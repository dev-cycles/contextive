module Ubictionary.LanguageServer.Hover

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let private noHoverResult = Task.FromResult(Hover())

let private termMatches word (t:Definitions.Term) = t.Name = word

let private getWordAtPosition (p:HoverParams) (getWord: TextDocument.WordGetter) =
    match p.TextDocument with
    | null -> None
    | document -> getWord (document.Uri.ToUri()) p.Position

let private getTermHoverContent (terms: Definitions.Term seq) =
    let term = Seq.head terms
    [Some <| sprintf "**%s**" term.Name; term.Definition]
    |> Seq.filter Option.isSome |> Seq.map (Option.defaultValue "")
    |> String.concat ": " 

let handler (termFinder: Definitions.Finder) (getWord: TextDocument.WordGetter) (p:HoverParams) (hc:HoverCapability) _ = 
    let wordAtPosition = getWordAtPosition p getWord
    match wordAtPosition with
    | None -> noHoverResult
    | Some(word) -> // TODO: actually find the correct position
        let terms = termFinder (termMatches word)
        match Seq.isEmpty terms with
        | true -> noHoverResult
        | false -> Task.FromResult(Hover(Contents = new MarkedStringsOrMarkupContent(MarkupContent(Kind = MarkupKind.Markdown, Value = getTermHoverContent terms))))

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities)  =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)