module Ubictionary.LanguageServer.Tests.HoverTests

open Expecto
open System.Threading.Tasks
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open TestClient

[<Tests>]
let hoverTests =
    testList "Hover Tests" [
        testAsync "Server response to hover request" {
            use! client = initTestClient

            let hoverParams = HoverParams()

            let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

            test <@ hover.Contents.HasMarkedStrings @>
        }
    ]