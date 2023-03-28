module Contextive.LanguageServer.Hover

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open Humanizer

let private (|EmptySeq|_|) a = if Seq.isEmpty a then Some () else None

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let private noHoverResult = null

let private termEquals (term:Definitions.Term) (word:string) =
    let normalisedTerm = term.Name.Replace(" ", "")
    let singularEquals = normalisedTerm.Equals(word, System.StringComparison.InvariantCultureIgnoreCase)
    let singularWord = word.Singularize(false, false)
    let pluralEquals = normalisedTerm.Equals(singularWord, System.StringComparison.InvariantCultureIgnoreCase)
    singularEquals || pluralEquals

let private termWordEquals (term:Definitions.Term) (word:Words.WordAndParts) = fst word |> termEquals term
let private termInParts (term:Definitions.Term) (word:Words.WordAndParts) = (snd word) |> Seq.exists (termEquals term)

let private termFilterForWords words =
    fun (t:Definitions.Term) -> 
        let wordMatchesTerm = termWordEquals t
        words |> Seq.exists wordMatchesTerm

let private filterRelevantTerms (terms: Definitions.Term seq) (wordsAndParts: Words.WordAndParts seq) =
    let exactTerms =
        wordsAndParts
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

let private emojify t = "📗 " + t
let private emphasise t = $"`{t}`"
let private define d =
    match d with
    | None -> "_undefined_"
    | Some(def) -> def

let private getHoverDefinition (term: Definitions.Term) =
    [term.Name |> emphasise |> emojify; term.Definition |> define]
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

let concatIfExists (separator:string) (lines:string option seq) =
    match lines |> Seq.choose id with
    | EmptySeq -> None
    | s -> s |> String.concat separator |> Some

let concatWithNewLinesIfExists = concatIfExists "\n\n"

let getTermHoverContent (terms: Definitions.Term seq) =
    [getHoverDefinition; getHoverUsageExamples]
    |> Seq.collect (fun p -> terms |> Seq.collect p)
    |> concatWithNewLinesIfExists

let private getContextHeading (context: Definitions.Context) =
    match context.Name with
    | null | "" -> None
    | _ -> Some $"### 💠 {context.Name} Context"

let private getContextDomainVisionStatement (context: Definitions.Context) =
    match context.DomainVisionStatement with
    | null | "" -> None
    | _ -> Some $"_Vision: {context.DomainVisionStatement}_"

let private getContextHover (wordsAndParts: Words.WordAndParts seq) (context: Definitions.Context) =
    let relevantTerms = filterRelevantTerms context.Terms wordsAndParts
    if Seq.length relevantTerms = 0 then
        None
    else
        [getTermHoverContent relevantTerms]
        |> Seq.append
            ([getContextHeading; getContextDomainVisionStatement]
            |> Seq.map (fun f -> f context))
        |> concatWithNewLinesIfExists

let private ContextSeparator = "\n\n***\n\n"

let private getContextsHoverContent (wordsAndParts: Words.WordAndParts seq) (contexts: Definitions.FindResult)  =
    contexts
    |> Seq.map (getContextHover wordsAndParts)
    |> concatIfExists ContextSeparator

let private hoverResult (wordsAndParts:  Words.WordAndParts seq) (contexts: Definitions.FindResult) =
    let content = getContextsHoverContent wordsAndParts contexts
    match content with
    | None -> noHoverResult
    | Some(c) -> Hover(Contents = (c |> markupContent))

let private hoverContentForWords (uri:string) (termFinder:Definitions.Finder) (wordsAndParts: Words.WordAndParts seq) = async {
        let! findResult = termFinder uri (termFilterForWords wordsAndParts)
        return
            if Seq.isEmpty findResult then
                noHoverResult
            else
                hoverResult wordsAndParts findResult
    }

let handler (termFinder: Definitions.Finder) (wordsGetter: TextDocument.WordGetter) (p:HoverParams) (hc:HoverCapability) _ =
    async {
        let wordAtPosition = getWordAtPosition p wordsGetter
        return!
            match wordAtPosition with
            | None -> async { return noHoverResult }
            | _ -> 
                let wordAndParts = Words.splitIntoWordAndParts wordAtPosition
                hoverContentForWords (p.TextDocument.Uri.ToString()) termFinder wordAndParts
                    
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities) =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)