module Ubictionary.LanguageServer.Tests.HoverTests

open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open TestClient

[<Tests>]
let hoverTests =
    testList "Hover Tests" [
        testAsync "Given no ubictionary and no document sync, server response to hover request with empty result" {
            use! client = initTestClient

            let hoverParams = HoverParams()

            let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

            test <@ not hover.Contents.HasMarkupContent @>
        }
    ]