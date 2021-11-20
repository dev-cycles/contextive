module Ubictionary.LanguageServer.Tests.HoverTests

open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open TestClient
open System.IO

[<Tests>]
let hoverTests =
    testList "Hover Tests" [
        testAsync "Given no ubictionary and no document sync, server response to hover request with empty result" {
            use! client = SimpleTestClient |> init

            let hoverParams = HoverParams()

            let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

            test <@ not hover.Contents.HasMarkupContent @>
        }

        testAsync "Given ubictionary and document sync, server response to hover request hover definition" {
            let fileName = "one"
            let config = [
                    Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.ubictionaryPathOptionsBuilder $"{fileName}.yml"
                ]

            use! client = TestClient(config) |> init

            let hoverParams = HoverParams()

            let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

            test <@ hover.Contents.HasMarkupContent @>
            test <@ hover.Contents.MarkupContent.Kind = MarkupKind.Markdown @>
            test <@ hover.Contents.MarkupContent.Value = "### firstTerm" @>
        }
    ]