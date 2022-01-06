module Contextly.LanguageServer.Tests.ConfigurationTests

open Expecto
open Swensen.Unquote
open System.IO
open TestClient
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open OmniSharp.Extensions.LanguageServer.Protocol.Models

let getCompletionLabels (client:ILanguageClient) = async {
    let textDocumentUri = $"file:///{System.Guid.NewGuid().ToString()}"

    client.TextDocument.DidOpenTextDocument(DidOpenTextDocumentParams(TextDocument = TextDocumentItem(
        LanguageId = "plaintext",
        Version = 0,
        Text = "",
        Uri = textDocumentUri
    )))

    let completionParams = CompletionParams(
        TextDocument = textDocumentUri,
        Position = Position(0, 0)
    )

    let! result = client.TextDocument.RequestCompletion(completionParams).AsTask() |> Async.AwaitTask

    return (result.Items |> Seq.map (fun x -> x.Label))
}

[<Tests>]
let definitionsTests =
    testList "Configuration  Tests" [
        testAsync "Can receive configuration value" {         
            let config = [
                Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                ConfigurationSection.contextlyPathOptionsBuilder "one.yml"
            ]

            use! client = TestClient(config) |> init

            let! completionLabels = getCompletionLabels client

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

            let setting = ConfigurationSection.jTokenFromMap <| Map[("path", path)]
            let configSection = ConfigurationSection.jTokenFromMap <| Map[("contextly", setting)]

            let didChangeConfig = DidChangeConfigurationParams(
                Settings = configSection
            )

            client.Workspace.DidChangeConfiguration(didChangeConfig)

            let! completionLabels = getCompletionLabels client

            test <@ (completionLabels, ["word1";"word2";"word3"]) ||> Seq.compareWith compare = 0 @>

        }
             
    ]