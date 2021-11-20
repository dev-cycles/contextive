module Ubictionary.LanguageServer.Hover

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let private noHoverResult = Task.FromResult(Hover())

let private termMatches _ = true
let private termToString (t:Definitions.Term) = t.name

let handler (termFinder: Definitions.Finder) (p:HoverParams) (hc:HoverCapability) _ = 
    match p.TextDocument with
    | null -> noHoverResult
    | _ -> 
        match termFinder termMatches termToString with
        | _ -> noHoverResult
        //| _ -> Task.FromResult(Hover(Contents = new MarkedStringsOrMarkupContent(MarkupContent(Kind = MarkupKind.Markdown, Value = ""))))

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities)  =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)