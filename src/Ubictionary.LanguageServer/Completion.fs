module Ubictionary.LanguageServer.Completion

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private completionList labels =
    CompletionList(labels |> Seq.map (fun l -> CompletionItem(Label=l)))

let private termMatches _ = true
let private termToString (t:Definitions.Term) = t.Name

let handler (termFinder: Definitions.Finder) (p:CompletionParams) (hc:CompletionCapability) _ = 
    Task.FromResult(completionList <| (termFinder termMatches |> Seq.map termToString))

let private registrationOptionsProvider (hc:CompletionCapability) (cc:ClientCapabilities)  =
    CompletionRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<CompletionRegistrationOptions, CompletionCapability>(registrationOptionsProvider)