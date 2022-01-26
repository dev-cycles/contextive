module Contextive.LanguageServer.Hover

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let private noHoverResult = null

let termMatch (term:Definitions.Term) word = term.Name.Equals(word, System.StringComparison.InvariantCultureIgnoreCase)

let private termMatches words (t:Definitions.Term) = 
    words |> List.exists (termMatch t)

let private getWordAtPosition (p:HoverParams) (getWords: TextDocument.WordGetter) =
    match p.TextDocument with
    | null -> []
    | document -> getWords (document.Uri.ToUri()) p.Position

let emphasise t = $"`{t}`"

let private getHoverDefinition (term: Definitions.Term) =
    [Some <| emphasise term.Name; term.Definition]
    |> Seq.choose id
    |> String.concat ": " 
    |> Some

let private hoverUsageExamplesToMarkdown (t:Definitions.Term) =
    t.Examples
    |> Seq.map (sprintf "\"%s\"")
    |> String.concat "\n\n"
    |> fun e -> $"***\n#### {emphasise t.Name} Usage Examples:\n" + e
    |> Some

let private getHoverUsageExamples = 
    function | {Definitions.Term.Examples = null} -> None
             | t -> hoverUsageExamplesToMarkdown t

let private getTermHoverContent (terms: Definitions.Term seq) =
    [getHoverDefinition; getHoverUsageExamples]
    |> Seq.collect (fun p -> terms |> Seq.map p)
    |> Seq.choose id
    |> String.concat "\n\n"

let private hoverResult (terms: Definitions.Term seq) =
    let content = getTermHoverContent terms |> markupContent
    Hover(Contents = content)

let private filterRelevantTerms (terms: Definitions.Term seq) (words: string list) =
    let fullWord = List.head words
    let exactTerm = terms |> Seq.filter (fun t -> termMatch t fullWord)
    if Seq.isEmpty exactTerm then
        terms
    else
        exactTerm

let handler (termFinder: Definitions.Finder) (wordsGetter: TextDocument.WordGetter) (p:HoverParams) (hc:HoverCapability) _ =
    async {
            let wordAtPosition = getWordAtPosition p wordsGetter
            return!
                match wordAtPosition with
                | [] -> async { return noHoverResult }
                | words -> // TODO: actually find the correct position
                    async {
                        let! terms = termFinder (termMatches words)
                        let relevantTerms = filterRelevantTerms terms words
                        return if Seq.isEmpty relevantTerms then
                                    noHoverResult
                                else
                                    hoverResult relevantTerms
                    }
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities) =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)