module Ubictionary.LanguageServer.Tests.CompletionTests

open Expecto
open Swensen.Unquote
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open TestClient

[<Tests>]
let completionTests =
    testSequenced <| testList "Completion Tests" [
        testAsync "Given no ubictionary respond with empty completion list " {
            use! client = SimpleTestClient |> init

            let completionParams = CompletionParams()

            let! result = client.TextDocument.RequestCompletion(completionParams).AsTask() |> Async.AwaitTask
        
            test <@ Seq.length result.Items = 0 @>
        }

        let testFileReader (name, fileName, expectedList) =
            testAsync $"Given {name} ubictionary, respond with expected completion list " {
                let config = [
                    Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.ubictionaryPathOptionsBuilder $"{fileName}.yml"
                ]

                use! client = TestClient(config) |> init
            
                let! result = client.TextDocument.RequestCompletion(CompletionParams()).AsTask() |> Async.AwaitTask

                let completionLabels = result.Items |> Seq.map (fun x -> x.Label)

                test <@ (completionLabels, expectedList) ||> Seq.compareWith compare = 0 @>
            }

        [
            ("first", "one", ["firstTerm";"secondTerm";"thirdTerm"])
            ("second", "two", ["word1";"word2";"word3"])
        ] |> List.map testFileReader |> testList "File reading tests"
    ]