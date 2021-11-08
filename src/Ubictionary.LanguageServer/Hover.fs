module Ubictionary.LanguageServer.Hover

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let handler (p:HoverParams) (hc:HoverCapability) _ = 
    Task.FromResult(Hover(Contents = markupContent "### Ubictionary Placeholder"))

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities)  =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)