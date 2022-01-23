module Contextive.LanguageServer.Tests.WatchedFilesTests

open System
open Expecto
open Swensen.Unquote
open System.IO
open System.Linq
open System.Collections.Generic
open Contextive.LanguageServer
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open TestClient
open WatchedFiles

let didChangeWatchedFiles (client:ILanguageClient) (uri:string) = 
    client.SendNotification(WorkspaceNames.DidChangeWatchedFiles, DidChangeWatchedFilesParams(
        Changes = Container(FileEvent(
            Uri = uri,
            Type = FileChangeType.Created
        ))
    ))

[<Tests>]
let watchedFileTests =
    testList "Watched File Tests" [

        let watcherIsForFile (fileName:string) (watchers:IEnumerable<FileSystemWatcher>) =
            watchers.Any(fun watcher -> watcher.GlobPattern.Contains(fileName) && watcher.Kind = WatchKind.Change)
                &&
            watchers.Any(fun watcher -> watcher.GlobPattern.Contains(fileName) && watcher.Kind = WatchKind.Create)

        testAsync "Server registers to receive watched file changes" {
            let registrationAwaiter = ConditionAwaiter.create()

            let config = [
                Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                ConfigurationSection.contextivePathOptionsBuilder $"one.yml"
                WatchedFiles.optionsBuilder registrationAwaiter
            ]

            let! client = TestClient(config) |> init

            let! registrationMsg = ConditionAwaiter.waitForAny registrationAwaiter 500

            test <@ client.ClientSettings.Capabilities.Workspace.DidChangeWatchedFiles.IsSupported @>

            test <@ registrationMsg.IsSome @>
            match registrationMsg with 
            | Some(Registered(_, opts)) -> 
                test <@ opts.Watchers |> watcherIsForFile "one.yml" @>
            | _ -> test <@ false @>
        }

        testAsync "Server registers to receive watched file changes after config update" {
            let registrationAwaiter = ConditionAwaiter.create()
            let mutable definitionsFile = "one.yml"
            let pathLoader():obj = definitionsFile
            let config = [
                Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                ConfigurationSection.contextivePathLoaderOptionsBuilder pathLoader
                WatchedFiles.optionsBuilder registrationAwaiter
            ]

            let! client = TestClient(config) |> init

            let! initialRegistrationMsg = ConditionAwaiter.waitForAny registrationAwaiter 500

            ConditionAwaiter.clear registrationAwaiter 500

            definitionsFile <- "three.yml"
            ConfigurationSection.didChange client definitionsFile

            let! secondRegistrationMsg = ConditionAwaiter.waitFor registrationAwaiter (fun m -> match m with | Registered(_) -> true | _ -> false) 500
            let! unregistrationMsg = ConditionAwaiter.waitFor registrationAwaiter (fun m -> match m with | Unregistered(_) -> true | _ -> false) 500

            match secondRegistrationMsg with
            | Some(Registered(_, opts)) ->
                test <@ opts.Watchers |> watcherIsForFile "three.yml" @>
            | _ -> failtest "no registration to watch after config changed"

            match initialRegistrationMsg, unregistrationMsg with 
            | Some(Registered(regoId, _)) , Some(Unregistered(unregoId)) -> 
                test <@ regoId = unregoId @>
            | _ -> failtest "registration of initial config not unregistered"
        }

        testAsync "Server reloads when the contextive file is created" {
            let newTerm = Guid.NewGuid().ToString()

            let relativePath = Path.Combine("fixtures", "completion_tests")
            let definitionsFile = "four.yml"
            let config = [
                Workspace.optionsBuilder relativePath
                ConfigurationSection.contextivePathOptionsBuilder definitionsFile
            ]
            let! client = TestClient(config) |> init

            let definitionsFileUri = Path.Combine(Directory.GetCurrentDirectory(), relativePath, definitionsFile)

            if (File.Exists(definitionsFileUri)) then
                File.Delete(definitionsFileUri)
            File.WriteAllText(definitionsFileUri, """contexts:
  - terms:
    - name: anewterm""")

            didChangeWatchedFiles client definitionsFileUri
            
            let! completionLabels = Completion.getCompletionLabels client

            File.Delete(definitionsFileUri)

            test <@ completionLabels |> Seq.contains "anewterm" @>
        }

        testAsync "Server reloads when the contextive file changes" {
            let newTerm = Guid.NewGuid().ToString()

            let relativePath = Path.Combine("fixtures", "completion_tests")
            let definitionsFile = "three.yml"
            let config = [
                Workspace.optionsBuilder relativePath
                ConfigurationSection.contextivePathOptionsBuilder definitionsFile
            ]
            let! client = TestClient(config) |> init

            let definitionsFileUri = Path.Combine(Directory.GetCurrentDirectory(), relativePath, definitionsFile)

            let existingContents = File.ReadAllText(definitionsFileUri)
            
            let newDefinition = $"{Environment.NewLine}    - name: {newTerm}"

            File.AppendAllText(definitionsFileUri, newDefinition)
            
            didChangeWatchedFiles client definitionsFileUri

            let! completionLabels = Completion.getCompletionLabels client

            File.WriteAllText(definitionsFileUri, existingContents)

            test <@ completionLabels |> Seq.contains newTerm @>
        }
    ]