module Contextly.LanguageServer.Tests.ConfigurationTests

open Expecto
open Swensen.Unquote
open System.IO
open TestClient

[<Tests>]
let definitionsTests =
    testList "Configuration  Tests" [
        testAsync "Can receive configuration value" {         
            let config = [
                Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                ConfigurationSection.contextlyPathOptionsBuilder "one.yml"
            ]

            use! client = TestClient(config) |> init

            let! completionLabels = Completion.getCompletionLabels client

            test <@ (completionLabels, ["firstTerm";"secondTerm";"thirdTerm"]) ||> Seq.compareWith compare = 0 @>
        }

        testAsync "Can handle configuration value changing" {
            let mutable path = "one.yml"

            let pathLoader():obj = path

            let config = [
                Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                ConfigurationSection.contextlyPathLoaderOptionsBuilder pathLoader
            ]

            use! client = TestClient(config) |> init

            path <- "two.yml"
            ConfigurationSection.didChange client path

            let! completionLabels = Completion.getCompletionLabels client

            test <@ (completionLabels, ["word1";"word2";"word3"]) ||> Seq.compareWith compare = 0 @>

        }
             
    ]