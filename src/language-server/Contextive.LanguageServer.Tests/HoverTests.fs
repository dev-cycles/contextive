module Contextive.LanguageServer.Tests.HoverTests

open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open System.IO
open Contextive.LanguageServer
open Contextive.Core.GlossaryFile
open Helpers.TestClient
open Contextive.LanguageServer.Tests.Helpers

module GlossaryFile = SubGlossaryHelper

[<Tests>]
let tests =
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

          let testHoverTermFound (text, position: Position, expectedTerm: string, fileName: string) =
              testAsync
                  $"Given definitions file '{fileName}' and file contents '{multiLineToSingleLine text}', server responds to hover request at Position {position} with '{expectedTerm}'" {
                  let config =
                      [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathBuilder $"{fileName}.yml" ]

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

          [ ("original", Position(0, 0), "original", "three")
            ("Original", Position(0, 0), "original", "three")
            ("original another", Position(0, 10), "another", "three")
            ("original another word", Position(0, 11), "another", "three")
            ("original\nanother", Position(1, 5), "another", "three")
            ("original\nword\nanother", Position(2, 4), "another", "three")
            ("original\r\nanother", Position(1, 5), "another", "three")
            ("original\r\nword\r\nanother", Position(2, 5), "another", "three")
            ("anotherWord", Position(0, 1), "another", "three")
            ("combined_word", Position(0, 1), "CombinedWord", "three")
            ("CombinedWordId", Position(0, 1), "CombinedWord", "three")
            ("combined-word", Position(0, 1), "CombinedWord", "three")
            ("AnotherCombinedWord", Position(0, 1), "another", "three")
            ("AnotherCombinedWord", Position(0, 1), "CombinedWord", "three")
            ("originals", Position(0, 0), "original", "three")
            ("octopi", Position(0, 0), "octopus", "three")
            ("snake_word", Position(0, 0), "snake_word", "three")
            ("kebab-word", Position(0, 0), "kebab-word", "three")
            ("single", Position(0, 0), "single", "empty_terms_list") ]
          |> List.map testHoverTermFound
          |> testList "Term found when hovering in opened docs at Positions"

          let testHoverTermNotFound (text, position: Position, fileName: string) =
              testAsync
                  $"Given definitions '{text}' and document open, server responds to hover request with no content in {position}" {

                  let config =
                      [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathBuilder $"{fileName}.yml" ]

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

          [ ("NotATerm", Position(0, 0), "one")
            ("firstTerm NotATerm", Position(0, 10), "one")
            ("    anothernotterm", Position(0, 0), "one")
            ("", Position(0, 0), "one")
            ("Something", Position(0, 0), "empty_terms_list") ]
          |> List.map testHoverTermNotFound
          |> testList "Nothing found when hovering"

          let testLessRelevantTermNotFound (text, position: Position, lessRelevantTerm: string) =
              testAsync
                  $"Given definitions '{text}' and document open, server responds to hover request without the less relevant '{lessRelevantTerm}' in {position}" {
                  let fileName = "three"

                  let config =
                      [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathBuilder $"{fileName}.yml" ]

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
                  test <@ not <| hover.Contents.MarkupContent.Value.Contains($"`{lessRelevantTerm}`") @>
              }

          [ ("CombinedWord", Position(0, 0), "combined")
            ("CombinedWord", Position(0, 0), "word") ]
          |> List.map testLessRelevantTermNotFound
          |> testList "Less relevant term NOT found when hovering"

          testAsync "Test hover with context info and no match" {
              let terms = []
              let foundToken = Some "term"

              let hoverHandler =
                  Hover.handler
                      (SubGlossaryHelper.mockTermNamesFinder
                          { Context.Default with
                              Name = "TestContext" }
                          terms)
                      (fun _ _ -> foundToken)

              let hoverParams =
                  HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))

              let! result = hoverHandler hoverParams null null |> Async.AwaitTask

              test <@ result = null @>
          } ]
