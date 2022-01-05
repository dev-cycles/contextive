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

[<Tests>]
let watchedFileTests =
    testList "Watched File Tests" [

        testAsync "Server registers to receive watched file changes" {

            let registrationAwaiter = ConditionAwaiter.create()

            let config = [
                        Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextlyPathOptionsBuilder $"one.yml"
                        WatchedFiles.optionsBuilder registrationAwaiter
                    ]

            do! TestClient(config) |> init |> Async.Ignore

            let! registrationOpts = ConditionAwaiter.waitForAny registrationAwaiter 500

            test <@ registrationOpts.IsSome @>
            test <@ registrationOpts.Value.Watchers.First().GlobPattern.Contains("one.yml") @>
            test <@ registrationOpts.Value.Watchers.First().Kind = WatchKind.Change @>
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

        testAsync "Server reloads when the contextly file changes after config update" {
            let newTerm = Guid.NewGuid().ToString()

            let registrationAwaiter = ConditionAwaiter.create()

            let relativePath = Path.Combine("fixtures", "completion_tests")
            let mutable definitionsFile = "two.yml"
            let pathLoader():obj = definitionsFile
            let config = [
                        Workspace.optionsBuilder relativePath
                        ConfigurationSection.contextlyPathLoaderOptionsBuilder pathLoader
                        WatchedFiles.optionsBuilder registrationAwaiter
                    ]
            let! client = TestClient(config) |> init

            definitionsFile <- "three.yml"

            let setting = ConfigurationSection.jTokenFromMap <| Map[("path", definitionsFile)]
            let configSection = ConfigurationSection.jTokenFromMap <| Map[("contextly", setting)]

            let didChangeConfig = DidChangeConfigurationParams(
                Settings = configSection
            )

            client.Workspace.DidChangeConfiguration(didChangeConfig)

            let! firstResult = client.TextDocument.RequestCompletion(CompletionParams()).AsTask() |> Async.AwaitTask

            let firstCompletionLabels = firstResult.Items |> Seq.map (fun x -> x.Label)

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

            let! registrationOpts = ConditionAwaiter.waitForAny registrationAwaiter 500

            test <@ registrationOpts.IsSome @>
            test <@ registrationOpts.Value.Watchers.First().GlobPattern.Contains("three.yml") @>
            test <@ registrationOpts.Value.Watchers.First().Kind = WatchKind.Change @>

            let! result = client.TextDocument.RequestCompletion(CompletionParams()).AsTask() |> Async.AwaitTask

            File.WriteAllText(definitionsFileUri, existingContents)

            let completionLabels = result.Items |> Seq.map (fun x -> x.Label)

            test <@ completionLabels |> Seq.contains newTerm @>
        }

    ]