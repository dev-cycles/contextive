module Contextive.LanguageServer.Tests.Helpers.WatchedFiles

open System.Threading.Tasks
open Newtonsoft.Json.Linq
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Client

type DidChangedWatchedFilesMessage =
    | Registered of id: string * DidChangeWatchedFilesRegistrationOptions
    | Unregistered of id: string

let optionsBuilder registrationAwaiter (b:LanguageClientOptions) =
    let received = ConditionAwaiter.received registrationAwaiter
    b
        .WithCapability(Capabilities.DidChangeWatchedFilesCapability())
        .OnUnregisterCapability(fun p -> 
            let reg = p.Unregistrations |> Seq.tryFind (fun r -> r.Method = WorkspaceNames.DidChangeWatchedFiles)
            match reg with
            | Some r -> 
                Unregistered(r.Id) |> received
            | _ -> ()
            Task.CompletedTask)
        .OnRegisterCapability(fun (p:RegistrationParams) ->
            let reg = p.Registrations |> Seq.tryFind (fun r -> r.Method = WorkspaceNames.DidChangeWatchedFiles)
            match reg with
            | Some r -> 
                let opts = (r.RegisterOptions :?> JToken).ToObject<DidChangeWatchedFilesRegistrationOptions>()
                Registered(r.Id, opts) |> received
                ()
            | None -> ()
            Task.CompletedTask
        ) |> ignore
    b