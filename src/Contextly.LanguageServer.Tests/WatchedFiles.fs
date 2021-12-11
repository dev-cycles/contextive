module Contextly.LanguageServer.Tests.WatchedFiles

open System.Threading.Tasks
open Newtonsoft.Json.Linq
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Client

let optionsBuilder registrationAwaiter (b:LanguageClientOptions) =
    b
        .WithCapability(Capabilities.DidChangeWatchedFilesCapability())
        .OnRegisterCapability(fun (p:RegistrationParams) ->
            let reg = p.Registrations |> Seq.tryFind (fun r -> r.Method = WorkspaceNames.DidChangeWatchedFiles)
            match reg with
            | Some r -> 
                let opts = (r.RegisterOptions :?> JToken).ToObject<DidChangeWatchedFilesRegistrationOptions>()
                ConditionAwaiter.received registrationAwaiter opts
                ()
            | None -> ()
            Task.CompletedTask
        ) |> ignore
    b