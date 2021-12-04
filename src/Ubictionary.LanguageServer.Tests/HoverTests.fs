module Ubictionary.LanguageServer.Tests.HoverTests

open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open Ubictionary.LanguageServer
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

        

        let testHoverTermFound (text, position: Position, expectedTerm: string) = 
            testAsync $"Given ubictionary and text '{text}', server responds to hover request with {expectedTerm} in {position}" {
                let fileName = "one"
                let config = [
                        Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.ubictionaryPathOptionsBuilder $"{fileName}.yml"
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
            ("firstTerm", Position(0, 0), "firstTerm")
            ("FirstTerm", Position(0, 0), "firstTerm")
            ("secondTerm thirdTerm", Position(0, 10), "secondTerm")
            ("secondTerm thirdTerm", Position(0, 11), "thirdTerm")
            ("secondTerm thirdTerm", Position(0, 12), "thirdTerm")
            ("secondTerm\nthirdTerm", Position(1, 5), "thirdTerm")
            ("secondTerm\nthirdTerm\nfirstTerm", Position(2, 4), "firstTerm")
            ("thirdTerm\r\nsecondTerm", Position(1, 5), "secondTerm")
            ("thirdTerm\r\nsecondTerm\r\nfirstTerm", Position(2, 5), "firstTerm")
        ] |> List.map testHoverTermFound |> testList "Term found when hovering in opened docs at Positions"

        let testHoverTermNotFound (text, position: Position) = 
            testAsync $"Given ubictionary and document open, server responds to hover request with no content in {position}" {
                let fileName = "one"
                let config = [
                        Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.ubictionaryPathOptionsBuilder $"{fileName}.yml"
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

                test <@ not hover.Contents.HasMarkupContent @>
            }

        [
            ("NotATerm", Position(0, 0))
            ("firstTerm NotATerm", Position(0, 10))
        ] |> List.map testHoverTermNotFound |> testList "Term not found when hovering"

        let testHoverDisplay (term: Definitions.Term, expectedHover) =
            testAsync $"Test hover format for {term.Name}" { 
                let hoverHandler = Hover.handler (fun _ -> [term]) (fun _ _ -> Some term.Name)

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
        
    ]