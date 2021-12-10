module Contextly.LanguageServer.Hover

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let private noHoverResult = Task.FromResult(Hover())

let private termMatches word (t:Definitions.Term) = t.Name.Equals(word, System.StringComparison.InvariantCultureIgnoreCase)

let private getWordAtPosition (p:HoverParams) (getWord: TextDocument.WordGetter) =
    match p.TextDocument with
    | null -> None
    | document -> getWord (document.Uri.ToUri()) p.Position

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
    Task.FromResult(Hover(Contents = content))

let handler (termFinder: Definitions.Finder) (getWord: TextDocument.WordGetter) (p:HoverParams) (hc:HoverCapability) _ = 
    let wordAtPosition = getWordAtPosition p getWord
    match wordAtPosition with
    | None -> noHoverResult
    | Some(word) -> // TODO: actually find the correct position
        let terms = termFinder (termMatches word)
        if Seq.isEmpty terms then
            noHoverResult
        else
            hoverResult terms

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities)  =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)