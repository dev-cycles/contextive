module Ubictionary.LanguageServer.Tests.CompletionTests

open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open TestClient

[<Tests>]
let completionTests =
    testList "Completion Tests" [
        testAsync "Given no ubictionary respond with empty completion list " {
            use! client = initTestClient

            let completionParams = CompletionParams()

            let! result = client.TextDocument.RequestCompletion(completionParams).AsTask() |> Async.AwaitTask
        
            test <@ Seq.length result.Items = 0 @>
        }
    ]