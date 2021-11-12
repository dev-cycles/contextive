module Ubictionary.LanguageServer.Tests.CompletionTests

open Expecto
open Swensen.Unquote
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open TestClient

[<Tests>]
let completionTests =
    testList "Completion Tests" [
        testAsync "Given no ubictionary respond with empty completion list " {
            use! client = SimpleTestClient |> init

            let completionParams = CompletionParams()

            let! result = client.TextDocument.RequestCompletion(completionParams).AsTask() |> Async.AwaitTask
        
            test <@ Seq.length result.Items = 0 @>
        }

        testAsync "Given ubictionary, respond with valid completion list " {
            let config = {
                WorkspaceFolderPath = Path.Combine("fixtures", "simple_ubictionary")
                ConfigurationSettings = Map [("path", "definitions.yml")]
            }

            use! client = TestClient(config) |> init
           
            let! result = client.TextDocument.RequestCompletion(CompletionParams()).AsTask() |> Async.AwaitTask

            let completionLabels = result.Items |> Seq.map (fun x -> x.Label)

            test <@ (completionLabels, ["firstTerm";"secondTerm";"thirdTerm"]) ||> Seq.compareWith compare = 0 @>
        }
    ]