module Contextive.LanguageServer.Hover

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let (|EmptySeq|_|) a = if Seq.isEmpty a then Some () else None

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let private noHoverResult = null

let termEquals (term:Definitions.Term) (word:string) =
    term
        .Name
        .Replace(" ", "")
        .Equals(word, System.StringComparison.InvariantCultureIgnoreCase)

let termWordEquals (term:Definitions.Term) (word:Words.WordAndParts) = fst word |> termEquals term
let termInParts (term:Definitions.Term) (word:Words.WordAndParts) = (snd word) |> Seq.exists (termEquals term)

let private termFilterForWords words =
    fun (t:Definitions.Term) -> 
        let wordMatchesTerm = termWordEquals t
        words |> Seq.exists wordMatchesTerm

let private filterRelevantTerms (terms: Definitions.Term seq) (words: Words.WordAndParts seq) =
    let exactTerms =
        words
        |> Seq.allPairs terms
        |> Seq.filter (fun (t, w) -> termWordEquals t w)
    let relevantTerms =
        exactTerms
        |> Seq.filter (fun (t, wAndP) -> 
            exactTerms
            |> Seq.except (seq {(t, wAndP)})
            |> Seq.exists (fun (_, w) -> termInParts t w)
            |> not)
        |> Seq.map fst
    match relevantTerms with
    | EmptySeq -> terms
    | _ -> relevantTerms

let private getWordAtPosition (p:HoverParams) (getWords: TextDocument.WordGetter) =
    match p.TextDocument with
    | null -> None
    | document -> getWords (document.Uri.ToUri()) p.Position

let emojify t = "📗 " + t
let emphasise t = $"`{t}`"

let private getHoverDefinition (term: Definitions.Term) =
    [term.Name |> emphasise |> emojify |> Some; term.Definition]
    |> Seq.choose id
    |> String.concat ": "
    |> Some
    |> Seq.singleton

let speechify usageExample = $"💬 \"{usageExample}\""

let private hoverUsageExamplesToMarkdown (t:Definitions.Term) =
    t.Examples
    |> Seq.map speechify
    |> Seq.append (Seq.singleton $"#### {emphasise t.Name} Usage Examples:")
    |> Seq.map Some

let private getHoverUsageExamples = 
    function | {Definitions.Term.Examples = null} -> Seq.empty
             | t -> hoverUsageExamplesToMarkdown t

let private getTermHoverContent (terms: Definitions.Term seq) =
    [getHoverDefinition; getHoverUsageExamples]
    |> Seq.collect (fun p -> terms |> Seq.collect p)

let getContextHeading (context: Definitions.Context) =
    match context.Name with
    | null | "" -> None
    | _ -> Some $"### 💠 {context.Name} Context"

let getContextDomainVisionStatement (context: Definitions.Context) =
    match context.DomainVisionStatement with
    | null | "" -> None
    | _ -> Some $"_Vision: {context.DomainVisionStatement}_"

let private getContextHover (words: Words.WordAndParts seq) (context: Definitions.Context) =
    let relevantTerms = filterRelevantTerms context.Terms words
    if Seq.length relevantTerms = 0 then
        ""
    else
        getTermHoverContent relevantTerms
        |> Seq.append
            ([getContextHeading; getContextDomainVisionStatement]
            |> Seq.map (fun f -> f context))
        |> Seq.choose id
        |> String.concat "\n\n"

let ContextSeparator = "\n\n***\n\n"

let private getContextsHoverContent (words: Words.WordAndParts seq) (contexts: Definitions.FindResult)  =
    contexts
    |> Seq.map (getContextHover words)
    |> String.concat ContextSeparator

let private hoverResult (words:  Words.WordAndParts seq) (contexts: Definitions.FindResult) =
    let content = getContextsHoverContent words contexts
    match content with
    | "" -> noHoverResult
    | _ -> Hover(Contents = (content |> markupContent))

let private hoverContentForWords (uri:string) (termFinder:Definitions.Finder) (words: Words.WordAndParts seq) = async {
        let! findResult = termFinder uri (termFilterForWords words)
        return
            if Seq.isEmpty findResult then
                noHoverResult
            else
                hoverResult words findResult
    }

let handler (termFinder: Definitions.Finder) (wordsGetter: TextDocument.WordGetter) (p:HoverParams) (hc:HoverCapability) _ =
    async {
        let wordAtPosition = getWordAtPosition p wordsGetter
        return!
            match wordAtPosition with
            | None -> async { return noHoverResult }
            | _ -> 
                let words = Words.split wordAtPosition
                hoverContentForWords (p.TextDocument.Uri.ToString()) termFinder words
                    
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities) =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)