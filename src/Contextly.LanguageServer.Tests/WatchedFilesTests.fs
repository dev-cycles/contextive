module Contextly.LanguageServer.Tests.WatchedFilesTests

open System
open Expecto
open Swensen.Unquote
open System.IO
open System.Linq
open Contextly.LanguageServer
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open TestClient
open WatchedFiles

[<Tests>]
let watchedFileTests =
    testList "Watched File Tests" [

        let watcherIsForFile (fileName:string) (watcher:FileSystemWatcher) =
            watcher.GlobPattern.Contains(fileName) && watcher.Kind = WatchKind.Change

        ftestAsync "Server registers to receive watched file changes" {
            let registrationAwaiter = ConditionAwaiter.create()

            let config = [
                Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                ConfigurationSection.contextlyPathOptionsBuilder $"one.yml"
                WatchedFiles.optionsBuilder registrationAwaiter
            ]

            let! client = TestClient(config) |> init

            let! registrationMsg = ConditionAwaiter.waitForAny registrationAwaiter 500

            test <@ client.ClientSettings.Capabilities.Workspace.DidChangeWatchedFiles.IsSupported @>

            test <@ registrationMsg.IsSome @>
            match registrationMsg with 
            | Some(Registered(_, opts)) -> test <@ opts.Watchers.First() |> watcherIsForFile "one.yml" @>
            | _ -> test <@ false @>
        }

        ftestAsync "Server registers to receive watched file changes after config update" {
            let registrationAwaiter = ConditionAwaiter.create()
            let mutable definitionsFile = "one.yml"
            let pathLoader():obj = definitionsFile
            let config = [
                Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                ConfigurationSection.contextlyPathLoaderOptionsBuilder pathLoader
                WatchedFiles.optionsBuilder registrationAwaiter
            ]

            let! client = TestClient(config) |> init

            let! initialRegistrationMsg = ConditionAwaiter.waitForAny registrationAwaiter 500

            ConditionAwaiter.clear registrationAwaiter 500

            definitionsFile <- "three.yml"
            client.Workspace.DidChangeConfiguration(DidChangeConfigurationParams())

            let! secondRegistrationMsg = ConditionAwaiter.waitFor registrationAwaiter (fun m -> match m with | Registered(_) -> true | _ -> false) 500
            let! unregistrationMsg = ConditionAwaiter.waitFor registrationAwaiter (fun m -> match m with | Unregistered(_) -> true | _ -> false) 500


            match secondRegistrationMsg with
            | Some(Registered(_, opts)) ->
                test <@ opts.Watchers.First() |> watcherIsForFile "three.yml" @>
            | _ -> failtest "no registration to watch after config changed"

            match initialRegistrationMsg, unregistrationMsg with 
            | Some(Registered(regoId, _)) , Some(Unregistered(unregoId)) -> 
                test <@ regoId = unregoId @>
            | _ -> failtest "registration of initial config not unregistered"
        }

        testAsync "Server reloads when the contextly file changes" {
            let newTerm = Guid.NewGuid().ToString()

            let relativePath = Path.Combine("fixtures", "completion_tests")
            let definitionsFile = "three.yml"
            let config = [
                Workspace.optionsBuilder relativePath
                ConfigurationSection.contextlyPathOptionsBuilder definitionsFile
            ]
            let! client = TestClient(config) |> init

            let definitionsFileUri = Path.Combine(Directory.GetCurrentDirectory(), relativePath, definitionsFile)

            let existingContents = File.ReadAllText(definitionsFileUri)
            
            let newDefinition = $"{Environment.NewLine}    - name: {newTerm}"

            File.AppendAllText(definitionsFileUri, newDefinition)
            
            client.SendNotification(WorkspaceNames.DidChangeWatchedFiles, DidChangeWatchedFilesParams(
                Changes = Container(FileEvent(
                    Uri = definitionsFileUri,
                    Type = FileChangeType.Changed
                ))
            ))

            let! result = client.TextDocument.RequestCompletion(CompletionParams()).AsTask() |> Async.AwaitTask

            File.WriteAllText(definitionsFileUri, existingContents)

            let completionLabels = result.Items |> Seq.map (fun x -> x.Label)

            test <@ completionLabels |> Seq.contains newTerm @>
        }
    ]