module Contextive.LanguageServer.Tests.HoverTests

open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open Contextive.LanguageServer
open TestClient
open System.IO

[<Tests>]
let hoverTests =
    testList "Hover Tests" [
        testAsync "Given no definitions and no document sync, server response to hover request with empty result" {
            use! client = SimpleTestClient |> init

            let hoverParams = HoverParams()

            let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

            test <@ hover = null @>
        }

        let multiLineToSingleLine (t:string) = t.Replace("\n", "\\n").Replace("\r", "\\r")

        let testHoverTermFound (text, position: Position, expectedTerm: string) = 
            testAsync $"Given definitions and text '{multiLineToSingleLine text}', server responds to hover request with {expectedTerm} in {position}" {
                let fileName = "three"
                let config = [
                        Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathOptionsBuilder $"{fileName}.yml"
                    ]

                use! client = TestClient(config) |> init

                let textDocumentUri = $"file:///{System.Guid.NewGuid().ToString()}"

                client.TextDocument.DidOpenTextDocument(DidOpenTextDocumentParams(TextDocument = TextDocumentItem(
                    LanguageId = "plaintext",
                    Version = 0,
                    Text = text,
                    Uri = textDocumentUri
                )))

                let hoverParams = HoverParams(
                    TextDocument = textDocumentUri,
                    Position = position
                )

                let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

                test <@ hover.Contents.HasMarkupContent @>
                test <@ hover.Contents.MarkupContent.Kind = MarkupKind.Markdown @>
                test <@ hover.Contents.MarkupContent.Value.Contains(expectedTerm) @>
            }

        [
            ("original", Position(0, 0), "original")
            ("Original", Position(0, 0), "original")
            ("original another", Position(0, 10), "another")
            ("original another word", Position(0, 11), "another")
            ("original\nanother", Position(1, 5), "another")
            ("original\nword\nanother", Position(2, 4), "another")
            ("original\r\nanother", Position(1, 5), "another")
            ("original\r\nword\r\nanother", Position(2, 5), "another")
            ("anotherWord", Position(0, 1), "another")
        ] |> List.map testHoverTermFound |> testList "Term found when hovering in opened docs at Positions"

        let testHoverTermNotFound (text, position: Position) = 
            testAsync $"Given definitions and document open, server responds to hover request with no content in {position}" {
                let fileName = "one"
                let config = [
                        Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathOptionsBuilder $"{fileName}.yml"
                    ]

                use! client = TestClient(config) |> init

                let textDocumentUri = $"file:///{System.Guid.NewGuid().ToString()}"

                client.TextDocument.DidOpenTextDocument(DidOpenTextDocumentParams(TextDocument = TextDocumentItem(
                    LanguageId = "plaintext",
                    Version = 0,
                    Text = text,
                    Uri = textDocumentUri
                )))

                let hoverParams = HoverParams(
                    TextDocument = textDocumentUri,
                    Position = position
                )

                let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

                test <@ hover = null @>
            }

        [
            ("NotATerm", Position(0, 0))
            ("firstTerm NotATerm", Position(0, 10))
        ] |> List.map testHoverTermNotFound |> testList "Term not found when hovering"

        let testHoverDisplay (term: Definitions.Term, expectedHover) =
            testAsync $"Test hover format for {term.Name}" { 
                let hoverHandler = Hover.handler (fun _ -> async { return [term] }) (fun _ _ -> [term.Name])

                let hoverParams = HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))
                let! result = hoverHandler hoverParams null null |> Async.AwaitTask

                test <@ result.Contents.MarkupContent.Value = expectedHover @>
            }
        [
            ({Definitions.Term.Default with Name = "firstTerm"; Definition = Some "The first term in our definitions list"},
                "**firstTerm**: The first term in our definitions list")
            ({Definitions.Term.Default with Name = "SecondTerm"},
                "**SecondTerm**")
            ({Definitions.Term.Default with Name = "ThirdTerm"; Examples = ResizeArray ["Do a thing"] },
                "**ThirdTerm**\n***\n#### Usage Examples:\n\"Do a thing\"")
        ] |> List.map testHoverDisplay |> testList "Term hover display"

        let testHoverOverMultiWord (term: string, foundWords: string list, expectedHover) =
            testAsync $"Test hover result for {term}" { 
                let termDefinition = {Definitions.Term.Default with Name = term}
                let hoverHandler = Hover.handler (fun _ -> async { return [termDefinition] }) (fun _ _ -> foundWords)

                let hoverParams = HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))
                let! result = hoverHandler hoverParams null null |> Async.AwaitTask

                test <@ result.Contents.MarkupContent.Value = expectedHover @>
            }
        [
            ("SecondTerm", ["SecondTerm"], "**SecondTerm**")            
            ("Second", ["SecondTerm"; "Second"; "Term"], "**Second**")
        ] |> List.map testHoverOverMultiWord |> testList "Term hover display over MultiWord"

    ]