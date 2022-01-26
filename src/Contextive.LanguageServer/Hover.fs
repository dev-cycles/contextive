module Contextive.LanguageServer.Hover

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let private noHoverResult = null

let private termMatches words (t:Definitions.Term) = 
    words |> List.exists (fun word -> t.Name.Equals(word, System.StringComparison.InvariantCultureIgnoreCase))

let private getWordAtPosition (p:HoverParams) (getWords: TextDocument.WordGetter) =
    match p.TextDocument with
    | null -> []
    | document -> getWords (document.Uri.ToUri()) p.Position

let private getHoverDefinition (term: Definitions.Term) =
    [Some <| $"**{term.Name}**"; term.Definition]
    |> Seq.choose id
    |> String.concat ": " 
    |> Some

let private hoverUsageExamplesToMarkdown (e:ResizeArray<string>) =
    e
    |> Seq.map (sprintf "\"%s\"")
    |> String.concat "\n\n"
    |> fun e -> "***\n#### Usage Examples:\n" + e
    |> Some

let private getHoverUsageExamples = 
    function | {Definitions.Term.Examples = null} -> None
             | t -> t.Examples |> hoverUsageExamplesToMarkdown

let private getTermHoverContent (terms: Definitions.Term seq) =
    let term = Seq.head terms
    [getHoverDefinition; getHoverUsageExamples]
    |> Seq.map (fun p -> p term)
    |> Seq.choose id
    |> String.concat "\n"

let private hoverResult (terms: Definitions.Term seq) =
    let content = getTermHoverContent terms |> markupContent
    Hover(Contents = content)

let handler (termFinder: Definitions.Finder) (wordsGetter: TextDocument.WordGetter) (p:HoverParams) (hc:HoverCapability) _ =
    async {
            let wordAtPosition = getWordAtPosition p wordsGetter
            return!
                match wordAtPosition with
                | [] -> async { return noHoverResult }
                | words -> // TODO: actually find the correct position
                    async {
                        let! terms = termFinder (termMatches words)
                        return if Seq.isEmpty terms then
                                    noHoverResult
                                else
                                    hoverResult terms
                    }
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities)  =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)