module Contextive.LanguageServer.Tests.E2e.HoverTests

open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open System.IO
open Contextive.LanguageServer
open Contextive.Core.GlossaryFile
open Tests.Helpers.TestClient
open Contextive.LanguageServer.Tests.Helpers

module GlossaryHelper = GlossaryHelper

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

          let testHoverTermFoundWithDefaultGlossary (text, position: Position, expectedTerm: string, fileName: string) =
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
            ("auslöser", Position(0, 0), "Auslöser", "three")
            ("ausloeser", Position(0, 0), "Auslöser", "three")
            ("père", Position(0, 0), "père", "three")
            ("pere", Position(0, 0), "père", "three")
            ("strasse", Position(0, 0), "Straße", "three")
            ("straße", Position(0, 0), "Straße", "three")
            ("strasse", Position(0, 0), "STRAẞE", "three")
            ("single", Position(0, 0), "single", "empty_terms_list") ]
          |> List.map testHoverTermFoundWithDefaultGlossary
          |> testList "Term found when hovering in opened docs at Positions"

          let testHoverTermFoundWithMultipleGlossaries
              (fileName: string, text, position: Position, expectedTerm: string)
              =
              testAsync
                  $"""Given editing file '{fileName.Replace(".", "_")}' and file contents '{multiLineToSingleLine text}', server responds to hover request at Position {position} with '{expectedTerm}'""" {

                  let workspaceRelative = Path.Combine("fixtures", "scanning_tests")
                  let config = [ Workspace.optionsBuilder <| workspaceRelative ]

                  use! client = TestClient(config) |> init

                  let workspaceBase =
                      Workspace.workspaceFolderPath workspaceRelative |> _.ToUri().ToString()

                  let textDocumentUri = Path.Combine(workspaceBase, fileName)

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

                  let! hover = client.TextDocument.RequestHover hoverParams |> Async.AwaitTask

                  test <@ hover.Contents.HasMarkupContent @>
                  test <@ hover.Contents.MarkupContent.Kind = MarkupKind.Markdown @>
                  test <@ hover.Contents.MarkupContent.Value.Contains expectedTerm @>
              }

          [ "inRoot.txt", "Root", Position(0, 0), "root"
            "folder1/folder1.cs", "folder1", Position(0, 0), "folder1"
            "folder1/folder1_root.cs", "root", Position(0, 0), "root"
            "folder1/nestedFolder/nested.cs", "nested", Position(0, 0), "nested"
            "folder1/nestedFolder/nested_folder1.cs", "folder1", Position(0, 0), "folder1"
            "folder1/nestedFolder/nested_root.cs", "root", Position(0, 0), "root"
            "folder2/hypothetical/nested_folder2.cs", "folder2", Position(0, 0), "folder2"
            "folder2/hypothetical/nested_root.cs", "root", Position(0, 0), "root" ]
          |> List.map testHoverTermFoundWithMultipleGlossaries
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
            ("peere", Position(0, 0), "three")
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
                      (GlossaryHelper.mockTermNamesFinder
                          { Context.Default with
                              Name = "TestContext" }
                          terms)
                      (fun _ _ -> foundToken)

              let hoverParams =
                  HoverParams(TextDocument = TextDocumentItem(Uri = System.Uri("file:///blah")))

              let! result = hoverHandler hoverParams null null |> Async.AwaitTask

              test <@ result = null @>
          }


          testAsync $"Hover is within performance limit with many very large glossaries" {
              let workspaceRelative = Path.Combine("fixtures", "performance")
              let config = [ Workspace.optionsBuilder <| workspaceRelative ]

              use! client = TestClient config |> init

              // Update this if the performance test glossaries in `fixtures/performance` are updated with the `generate_perf_glossaries.fsx` script.
              let sampleTerm = "Cake switch thunder"

              let workspaceBase =
                  Workspace.workspaceFolderPath workspaceRelative |> _.ToUri().ToString()

              let textDocumentUri = Path.Combine(workspaceBase, "test.txt")

              client.TextDocument.DidOpenTextDocument(
                  DidOpenTextDocumentParams(
                      TextDocument =
                          TextDocumentItem(
                              LanguageId = "plaintext",
                              Version = 0,
                              Text = sampleTerm.Replace(" ", "_").ToLower(),
                              Uri = textDocumentUri
                          )
                  )
              )

              let hoverParams =
                  HoverParams(TextDocument = textDocumentUri, Position = Position(0, 1))

              let sw = System.Diagnostics.Stopwatch()
              sw.Start()
              let! hover = client.TextDocument.RequestHover(hoverParams) |> Async.AwaitTask
              sw.Stop()

              test <@ hover.Contents.HasMarkupContent @>
              test <@ hover.Contents.MarkupContent.Kind = MarkupKind.Markdown @>
              test <@ hover.Contents.MarkupContent.Value.Contains sampleTerm @>
              test <@ sw.Elapsed < System.TimeSpan.FromSeconds 6 @>
          } ]
