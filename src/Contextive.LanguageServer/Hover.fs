module Contextive.LanguageServer.Hover

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let private noHoverResult = null

let termWordEquals (term:Definitions.Term) word = term.Name.Equals(word, System.StringComparison.InvariantCultureIgnoreCase)

let private termFilterForWords words =
    fun (t:Definitions.Term) -> 
        let wordMatchesTerm = termWordEquals t
        words |> List.exists wordMatchesTerm

let private filterRelevantTerms (terms: Definitions.Term seq) (words: string list) =
    let fullWord = List.head words
    let exactTerm = terms |> Seq.filter (fun t -> termWordEquals t fullWord)
    if Seq.isEmpty exactTerm then
        terms
    else
        exactTerm

let private getWordAtPosition (p:HoverParams) (getWords: TextDocument.WordGetter) =
    match p.TextDocument with
    | null -> []
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

let private getContextHover (words: string list) (context: Definitions.Context) =
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

let private getContextsHoverContent (words: string list) (contexts: Definitions.FindResult)  =
    contexts
    |> Seq.map (getContextHover words)
    |> String.concat ContextSeparator

let private hoverResult (words: string list) (contexts: Definitions.FindResult) =
    let content = getContextsHoverContent words contexts
    match content with
    | "" -> noHoverResult
    | _ -> Hover(Contents = (content |> markupContent))

let private hoverContentForWords (uri:string) (termFinder:Definitions.Finder) (words: string list) = async {
        let! findResult = termFinder uri (termFilterForWords words)
        return
            if Seq.isEmpty findResult then
                noHoverResult
            else
                hoverResult words findResult
    }

let handler (termFinder: Definitions.Finder) (wordsGetter: TextDocument.WordGetter) (p:HoverParams) (hc:HoverCapability) _ =
    async {
        let wordsAtPosition = getWordAtPosition p wordsGetter
        return!
            match wordsAtPosition with
            | [] -> async { return noHoverResult }
            | words -> hoverContentForWords (p.TextDocument.Uri.ToString()) termFinder words
                    
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities) =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)