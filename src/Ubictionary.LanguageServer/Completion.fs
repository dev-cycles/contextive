module Ubictionary.LanguageServer.Completion

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let completionList labels =
    CompletionList(labels |> Seq.map (fun l -> CompletionItem(Label=l)))

let handler (p:CompletionParams) (hc:CompletionCapability) _ = 
    Task.FromResult(completionList ["firstTerm";"secondTerm";"thirdTerm"])

let private registrationOptionsProvider (hc:CompletionCapability) (cc:ClientCapabilities)  =
    CompletionRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<CompletionRegistrationOptions, CompletionCapability>(registrationOptionsProvider)