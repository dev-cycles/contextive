module Contextive.LanguageServer.Tests.HoverTests

open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open Contextive.LanguageServer
open Contextive.LanguageServer.Definitions
open TestClient
open System.IO

module DH = Contextive.LanguageServer.Tests.Definitions

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
            ("combined_word", Position(0, 1), "CombinedWord")
            ("CombinedWordId", Position(0, 1), "CombinedWord")
            ("AnotherCombinedWord", Position(0, 1), "another")
            ("AnotherCombinedWord", Position(0, 1), "CombinedWord")
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

        let testHoverDisplay (terms: Term list, (foundWords:string list), (expectedHover:string)) =
            testAsync $"Test hover format when hovering over {foundWords} and definitions are {terms |> List.map (fun t -> t.Name)}" { 
                let foundWordsAndParts = foundWords |> List.map (fun w -> (w, seq {w}))
                let hoverHandler = Hover.handler (DH.mockDefinitionsFinder Context.Default terms) (fun _ _ -> foundWordsAndParts)

                let hoverParams = HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))
                let! result = hoverHandler hoverParams null null |> Async.AwaitTask

                test <@ result.Contents.MarkupContent.Value.Contains(expectedHover) @>
            }
        [
            ([{Term.Default with Name = "firstTerm"; Definition = Some "The first term in our definitions list"}],
                ["firstTerm"],
                "📗 `firstTerm`: The first term in our definitions list")
            ([{Term.Default with Name = "SecondTerm"}],
                ["secondTerm"],
                "📗 `SecondTerm`")
            ([{Term.Default with Name = "ThirdTerm"; Examples = ResizeArray ["Do a thing"] }],
                ["thirdTerm"],
                "\
📗 `ThirdTerm`

#### `ThirdTerm` Usage Examples:

💬 \"Do a thing\"")
            ([{Term.Default with Name = "SecondTerm"}; {Term.Default with Name = "ThirdTerm"}],
                ["secondTerm"],
                "📗 `SecondTerm`")
            ([{Term.Default with Name = "Second"}; {Term.Default with Name = "Term"}],
                ["secondTerm"; "second"; "term"],
                "\
📗 `Second`

📗 `Term`\
                ")
            ([{Term.Default with Name = "First"; Examples = ResizeArray ["Do a thing"] }; {Term.Default with Name = "Term"}],
                ["firstTerm"; "first"; "term"],
                "\
📗 `First`

📗 `Term`

#### `First` Usage Examples:

💬 \"Do a thing\"")
            ([{Term.Default with Name = "Third"; Examples = ResizeArray ["Do a thing"] }; {Term.Default with Name = "Term"; Examples = ResizeArray ["Do something else"]}],
                ["thirdTerm"; "third"; "term"],
                "\
📗 `Third`

📗 `Term`

#### `Third` Usage Examples:

💬 \"Do a thing\"

#### `Term` Usage Examples:

💬 \"Do something else\"")
        ] |> List.map testHoverDisplay |> testList "Term hover display"

        let testHoverOverMultiWord (terms: string list, foundWords: TextDocument.WordAndParts list, expectedHover) =
            let foundWordsList = sprintf "%A" foundWords
            testAsync $"Test hover result with terms {terms} over word split {foundWordsList}" {
                let hoverHandler = Hover.handler (DH.mockTermsFinder Context.Default terms) (fun _ _ -> foundWords)

                let hoverParams = HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))
                let! result = hoverHandler hoverParams null null |> Async.AwaitTask

                test <@ result.Contents.MarkupContent.Value = expectedHover @>
            }
        [
            (["SecondTerm"], [("Second", seq{"Second"}); ("Term", seq{"Term"}); ("SecondTerm",seq{"Second";"Term"})], "📗 `SecondTerm`")            
            (["Second"], [("Second", seq{"Second"}); ("Term", seq{"Term"}); ("SecondTerm",seq{"Second";"Term"})], "📗 `Second`")
            (["SecondTerm";"Second";"Term"], [("Second", seq{"Second"}); ("Term", seq{"Term"}); ("SecondTerm",seq{"Second";"Term"})], "📗 `SecondTerm`")
            (["ThirdTerm";"Third";"Term"], [("Third",seq{"Third"}); ("Term",seq{"Term"}); ("ThirdTerm",seq{"Third";"Term"})], "📗 `ThirdTerm`")
            (["ThirdTerm";"Third";"Term"], [("Third",seq{"Third"}); ("Term",seq{"Term"}); ("Id",seq{"Id"}); ("ThirdTerm",seq{"Third";"Term"}); ("TermId",seq{"Term";"Id"}); ("ThirdTermId",seq{"Third";"Term";"Id"})], "📗 `ThirdTerm`")
        ] |> List.map testHoverOverMultiWord |> testList "Term hover display over MultiWord"

        testAsync $"Test hover with context info" {
            let terms = ["term"]
            let foundWords = [("term",seq{"term"})]
            let hoverHandler =
                Hover.handler (DH.mockTermsFinder ({Context.Default with Name = "TestContext"; DomainVisionStatement="supporting the test"}) terms) (fun _ _ -> foundWords)

            let hoverParams = HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))
            let! result = hoverHandler hoverParams null null |> Async.AwaitTask

            let expectedHover = "### 💠 TestContext Context

_Vision: supporting the test_

📗 `term`"

            test <@ result.Contents.MarkupContent.Value = expectedHover @>
        }



        testAsync $"Test hover with multiple context info" {
            let terms = ["term"]
            let foundWords = [("term",seq{"term"})]
            let contexts = seq {
                {Context.Default with Name = "Test"}
                {Context.Default with Name = "Other"}
            }

            let hoverHandler = Hover.handler (DH.mockMultiContextTermsFinder contexts terms) (fun _ _ -> foundWords)

            let hoverParams = HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))
            let! result = hoverHandler hoverParams null null |> Async.AwaitTask

            let expectedHover = "### 💠 Test Context

📗 `term`

***

### 💠 Other Context

📗 `term`"

            test <@ result.Contents.MarkupContent.Value = expectedHover @>
        }

        testAsync $"Test hover with context info and no match" {
            let terms = []
            let foundWords = [("term",seq{"term"})]
            let hoverHandler =
                Hover.handler (DH.mockTermsFinder ({Context.Default with Name = "TestContext"}) terms) (fun _ _ -> foundWords)

            let hoverParams = HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))
            let! result = hoverHandler hoverParams null null |> Async.AwaitTask

            test <@ result = null @>
        }

    ]