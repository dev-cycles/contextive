module Contextive.LanguageServer.Tests.HoverTests

open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open System.IO
open Contextive.LanguageServer
open Contextive.Core.Definitions
open Helpers.TestClient
open Contextive.LanguageServer.Tests.Helpers

module DH = Helpers.Definitions

[<Tests>]
let hoverTests =
    testList
        "LanguageServer.Hover Tests"
        [

          testAsync "Given no definitions and no document sync, server response to hover request with empty result" {
              use! client = SimpleTestClient |> init

              let hoverParams = HoverParams()

              let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

              test <@ hover = null @>
          }

          let multiLineToSingleLine (t: string) =
              t.Replace("\n", "\\n").Replace("\r", "\\r")

          let testHoverTermFound (text, position: Position, expectedTerm: string) =
              testAsync
                  $"Given definitions and text '{multiLineToSingleLine text}', server responds to hover request with {expectedTerm} in {position}" {
                  let fileName = "three"

                  let config =
                      [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathOptionsBuilder $"{fileName}.yml" ]

                  use! client = TestClient(config) |> init

                  let textDocumentUri = $"file:///{System.Guid.NewGuid().ToString()}"

                  client.TextDocument.DidOpenTextDocument(
                      DidOpenTextDocumentParams(
                          TextDocument =
                              TextDocumentItem(
                                  LanguageId = "plaintext",
                                  Version = 0,
                                  Text = text,
                                  Uri = textDocumentUri
                              )
                      )
                  )

                  let hoverParams = HoverParams(TextDocument = textDocumentUri, Position = position)

                  let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

                  test <@ hover.Contents.HasMarkupContent @>
                  test <@ hover.Contents.MarkupContent.Kind = MarkupKind.Markdown @>
                  test <@ hover.Contents.MarkupContent.Value.Contains(expectedTerm) @>
              }

          [ ("original", Position(0, 0), "original")
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
            ("originals", Position(0, 0), "original")
            ("octopi", Position(0, 0), "octopus") ]
          |> List.map testHoverTermFound
          |> testList "Term found when hovering in opened docs at Positions"

          let testHoverTermNotFound (text, position: Position) =
              testAsync
                  $"Given definitions '{text}' and document open, server responds to hover request with no content in {position}" {
                  let fileName = "one"

                  let config =
                      [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathOptionsBuilder $"{fileName}.yml" ]

                  use! client = TestClient(config) |> init

                  let textDocumentUri = $"file:///{System.Guid.NewGuid().ToString()}"

                  client.TextDocument.DidOpenTextDocument(
                      DidOpenTextDocumentParams(
                          TextDocument =
                              TextDocumentItem(
                                  LanguageId = "plaintext",
                                  Version = 0,
                                  Text = text,
                                  Uri = textDocumentUri
                              )
                      )
                  )

                  let hoverParams = HoverParams(TextDocument = textDocumentUri, Position = position)

                  let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask

                  test <@ hover = null @>
              }

          [ ("NotATerm", Position(0, 0))
            ("firstTerm NotATerm", Position(0, 10))
            ("    anothernotterm", Position(0, 0))
            ("", Position(0, 0)) ]
          |> List.map testHoverTermNotFound
          |> testList "Term not found when hovering"

          let testHoverDisplay (terms: Term list, (wordAtPosition: string), (expectedHover: string)) =
              testAsync
                  $"Test hover format when hovering over {wordAtPosition} and definitions are {terms |> List.map (fun t -> t.Name)}" {
                  let hoverHandler =
                      Hover.handler (DH.mockDefinitionsFinder Context.Default terms) (fun _ _ -> Some wordAtPosition)

                  let hoverParams =
                      HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))

                  let! result = hoverHandler hoverParams null null |> Async.AwaitTask

                  test <@ result.Contents.MarkupContent.Value.ReplaceLineEndings().Contains(expectedHover) @>
              }

          [ ([ { Term.Default with
                   Name = "firstTerm"
                   Definition = Some "The first term in our definitions list" } ],
             "firstTerm",
             "📗 `firstTerm`: The first term in our definitions list")

            ([ { Term.Default with
                   Name = "termWithAlias"
                   Aliases = ResizeArray [ "aliasOfTerm" ] } ],
             "aliasOfTerm",
             "📗 `termWithAlias`: _undefined_")

            ([ { Term.Default with
                   Name = "SecondTerm" } ],
             "secondTerm",
             "📗 `SecondTerm`: _undefined_")
            ([ { Term.Default with
                   Name = "ThirdTerm"
                   Examples = ResizeArray [ "Do a thing" ] } ],
             "thirdTerm",
             "\
📗 `ThirdTerm`: _undefined_

#### `ThirdTerm` Usage Examples:

💬 \"Do a thing\"")
            ([ { Term.Default with
                   Name = "SecondTerm" }
               { Term.Default with Name = "ThirdTerm" } ],
             "secondTerm",
             "📗 `SecondTerm`")
            ([ { Term.Default with Name = "Second" }; { Term.Default with Name = "Term" } ],
             "secondTerm",
             "\
📗 `Second`: _undefined_

📗 `Term`: _undefined_\
                ")
            ([ { Term.Default with
                   Name = "First"
                   Examples = ResizeArray [ "Do a thing" ] }
               { Term.Default with Name = "Term" } ],
             "firstTerm",
             "\
📗 `First`: _undefined_

📗 `Term`: _undefined_

#### `First` Usage Examples:

💬 \"Do a thing\"")
            ([ { Term.Default with
                   Name = "Third"
                   Examples = ResizeArray [ "Do a thing" ] }
               { Term.Default with
                   Name = "Term"
                   Examples = ResizeArray [ "Do something else" ] } ],
             "thirdTerm",
             "\
📗 `Third`: _undefined_

📗 `Term`: _undefined_

#### `Third` Usage Examples:

💬 \"Do a thing\"

#### `Term` Usage Examples:

💬 \"Do something else\"") ]
          |> List.map testHoverDisplay
          |> testList "Term hover display"

          let testHoverOverMultiWord (terms: string list, tokenAtPosition: string, expectedHover) =
              testAsync $"Test hover result with terms {terms} over token split {tokenAtPosition}" {
                  let hoverHandler =
                      Hover.handler (DH.mockTermsFinder Context.Default terms) (fun _ _ -> Some tokenAtPosition)

                  let hoverParams =
                      HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))

                  let! result = hoverHandler hoverParams null null |> Async.AwaitTask

                  test <@ result.Contents.MarkupContent.Value = expectedHover @>
              }

          [ ([ "SecondTerm" ], "SecondTerm", "📗 `SecondTerm`: _undefined_")
            ([ "Second Term" ], "SecondTerm", "📗 `Second Term`: _undefined_")
            ([ "Second" ], "SecondTerm", "📗 `Second`: _undefined_")
            ([ "SecondTerm"; "Second"; "Term" ], "SecondTerm", "📗 `SecondTerm`: _undefined_")
            ([ "ThirdTerm"; "Third"; "Term" ], "ThirdTerm", "📗 `ThirdTerm`: _undefined_")
            ([ "ThirdTerm"; "Third"; "Term" ], "ThirdTermId", "📗 `ThirdTerm`: _undefined_") ]
          |> List.map testHoverOverMultiWord
          |> testList "Term hover display over MultiWord"

          testAsync $"Test hover with context info" {
              let terms = [ "term" ]
              let foundToken = Some "term"

              let hoverHandler =
                  Hover.handler
                      (DH.mockTermsFinder
                          ({ Context.Default with
                              Name = "TestContext"
                              DomainVisionStatement = "supporting the test" })
                          terms)
                      (fun _ _ -> foundToken)

              let hoverParams =
                  HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))

              let! result = hoverHandler hoverParams null null |> Async.AwaitTask

              let expectedHover =
                  "### 💠 TestContext Context

_Vision: supporting the test_

📗 `term`: _undefined_"

              test <@ result.Contents.MarkupContent.Value.ReplaceLineEndings() = expectedHover @>
          }

          testAsync $"Test hover with multiple context info" {
              let terms = [ "term" ]
              let foundToken = Some "term"

              let contexts =
                  seq {
                      { Context.Default with Name = "Test" }
                      { Context.Default with Name = "Other" }
                  }

              let hoverHandler =
                  Hover.handler (DH.mockMultiContextTermsFinder contexts terms) (fun _ _ -> foundToken)

              let hoverParams =
                  HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))

              let! result = hoverHandler hoverParams null null |> Async.AwaitTask

              let expectedHover =
                  "### 💠 Test Context

📗 `term`: _undefined_

***

### 💠 Other Context

📗 `term`: _undefined_"

              test <@ result.Contents.MarkupContent.Value.ReplaceLineEndings() = expectedHover @>
          }

          testAsync $"Test hover with context info and no match" {
              let terms = []
              let foundToken = Some "term"

              let hoverHandler =
                  Hover.handler
                      (DH.mockTermsFinder
                          ({ Context.Default with
                              Name = "TestContext" })
                          terms)
                      (fun _ _ -> foundToken)

              let hoverParams =
                  HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))

              let! result = hoverHandler hoverParams null null |> Async.AwaitTask

              test <@ result = null @>
          }

          ]
