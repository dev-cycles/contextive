module Ubictionary.LanguageServer.Tests.WatchedFilesTests

open Expecto
open Swensen.Unquote
open System.IO
open System.Linq
open Ubictionary.LanguageServer
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol
open TestClient

[<Tests>]
let watchedFileTests =
    testList "Watched File Tests" [

        testAsync "Server registers to receive watched file changes" {

            let registrationAwaiter = ConditionAwaiter.create()

            let config = [
                        Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.ubictionaryPathOptionsBuilder $"one.yml"
                        WatchedFiles.optionsBuilder registrationAwaiter
                    ]

            do! TestClient(config) |> init |> Async.Ignore

            let! registrationOpts = ConditionAwaiter.waitForAny registrationAwaiter 500

            test <@ registrationOpts.IsSome @>
            test <@ registrationOpts.Value.Watchers.First().GlobPattern.Contains("one.yml") @>
            test <@ registrationOpts.Value.Watchers.First().Kind = WatchKind.Change @>
        }

    ]