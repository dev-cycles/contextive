module Contextive.LanguageServer.Tests.E2e.CompletionTests

open Expecto
open Swensen.Unquote
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open Contextive.LanguageServer
open Contextive.Core.GlossaryFile
open Contextive.LanguageServer.Tests.Helpers
open Contextive.LanguageServer.Tests.Helpers.TestClient

[<Tests>]
let tests =
    testList
        "LanguageServer.Completion Tests"
        [ testAsync "Given no contextive respond with empty completion list " {
              use! client = SimpleTestClient |> init

              let! labels = Completion.getCompletionLabels client

              test <@ Seq.length labels = 0 @>
          }


          let testCompletionsWithDefaultGlossary (fileName, text, position, expectedCompletionLabels) =
              testAsync
                  $"Given {fileName} glossary, with content '{text}' at position {position} respond with expected completion list " {
                  let config =
                      [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathBuilder $"{fileName}.yml" ]

                  use! client = TestClient(config) |> init

                  let textDocumentUri = $"file:///{System.Guid.NewGuid().ToString()}"

                  let! result = client |> Completion.getCompletionFromText text textDocumentUri position

                  test <@ result.IsIncomplete @>

                  let completionLabels = Completion.getLabels result

                  test <@ (completionLabels, expectedCompletionLabels) ||> Seq.compareWith compare = 0 @>
              }

          [ "one", "", Position(0, 0), Fixtures.One.expectedCompletionLabels
            "two", "", Position(0, 0), Fixtures.Two.expectedCompletionLabels
            "two", "W", Position(0, 1), Fixtures.Two.expectedCompletionLabelsPascal
            "two", "WO", Position(0, 2), Fixtures.Two.expectedCompletionLabelsUPPER ]
          |> List.map testCompletionsWithDefaultGlossary
          |> testList "Test Completions with default glossary only"

          let testCompletionsWithMultipleGlossaries (fileName: string, text, position, expectedCompletionLabels) =
              testAsync
                  $"""Given {fileName.Replace(".", "_")} path being edited, with content '{text}' at position {position} respond with expected completion list """ {

                  let workspaceRelative = Path.Combine("fixtures", "scanning_tests")

                  let config =
                      [ Workspace.optionsBuilder <| workspaceRelative
                        ConfigurationSection.contextivePathBuilder $"{fileName}.yml" ]

                  let workspaceBase =
                      Workspace.workspaceFolderPath workspaceRelative |> _.ToUri().ToString()

                  use! client = TestClient(config) |> init

                  let textDocumentUri = Path.Combine(workspaceBase, fileName)

                  let! result = client |> Completion.getCompletionFromText text textDocumentUri position

                  test <@ result.IsIncomplete @>

                  let completionLabels = Completion.getLabels result

                  test <@ Set.ofSeq completionLabels = Set.ofList expectedCompletionLabels @>
              }

          [ "test.txt", "", Position(0, 0), [ "root" ]
            "folder1/file.cs", "", Position(0, 0), [ "folder1"; "Folder1"; "folder_1"; "folder-1"; "root" ]
            "folder1/nestedFolder/file.cs",
            "",
            Position(0, 0),
            [ "folder1"; "Folder1"; "folder_1"; "folder-1"; "nested"; "root" ]
            "folder2/hypothetical/file.cs", "", Position(0, 0), [ "folder2"; "Folder2"; "folder_2"; "folder-2"; "root" ] ]
          |> List.map testCompletionsWithMultipleGlossaries
          |> testList "Test Completions with multiple glossaries"

          let singleWordCompletion (term, tokenAtPosition: string option, expectedLabel: string) =
              testCase $"Completion of \"{term}\" with {tokenAtPosition} at position, returns \"{expectedLabel}\""
              <| fun () ->
                  let finder: Finder = GlossaryHelper.mockTermNamesFinder Context.Default [ term ]

                  let tokenFinder: TextDocument.TokenFinder = fun _ _ -> tokenAtPosition

                  let completionLabels =
                      (Completion.handler finder tokenFinder Completion.defaultParams null null)
                          .Result
                      |> Completion.getLabels

                  test <@ (completionLabels, seq { expectedLabel }) ||> Seq.compareWith compare = 0 @>

          [ "term", Some "", "term"
            "Term", Some "", "Term"
            "term", Some "t", "term"
            "Term", Some "t", "term"
            "term", Some "T", "Term"
            "term", Some "Te", "Term"
            "term", Some "TE", "TERM"
            "term", Some "TEr", "Term"
            "term", None, "term" ]
          |> List.map singleWordCompletion
          |> testList "Single Word Completion"


          let multiWordCompletion (term, tokenAtPosition: string option, expectedCompletionLabels: string seq) =
              let expectedCompletionLabelsList =
                  sprintf "%A" <| Seq.toList expectedCompletionLabels

              testCase
                  $"Completion of \"{term}\" with {tokenAtPosition} at position, returns \"{expectedCompletionLabelsList}\""
              <| fun () ->
                  let finder: Finder = GlossaryHelper.mockTermNamesFinder Context.Default [ term ]

                  let tokenFinder: TextDocument.TokenFinder = fun _ _ -> tokenAtPosition

                  let completionLabels =
                      (Completion.handler finder tokenFinder Completion.defaultParams null null)
                          .Result
                      |> Completion.getLabels

                  test <@ (completionLabels, expectedCompletionLabels) ||> Seq.compareWith compare = 0 @>

          [ "Multi Word",
            Some "",
            seq {
                "multiWord"
                "MultiWord"
                "multi_word"
                "multi-word"
            }
            "Multi Word",
            Some "m",
            seq {
                "multiWord"
                "multi_word"
            }
            "Multi Word",
            Some "M",
            seq {
                "MultiWord"
                "MULTI_WORD"
            }
            "Multi Word",
            Some "MU",
            seq {
                "MULTIWORD"
                "MULTI_WORD"
            }
            "multi word",
            Some "",
            seq {
                "multiWord"
                "MultiWord"
                "multi_word"
                "multi-word"
            }
            "multi word",
            Some "m",
            seq {
                "multiWord"
                "multi_word"
            }
            "multi word",
            Some "M",
            seq {
                "MultiWord"
                "MULTI_WORD"
            }
            "multi word",
            Some "MU",
            seq {
                "MULTIWORD"
                "MULTI_WORD"
            }
            "Один Два",
            Some "О",
            seq {
                "ОдинДва"
                "ОДИН_ДВА"
            }
            "Один Два",
            Some "о",
            seq {
                "одинДва"
                "один_два"
            } ]
          |> List.map multiWordCompletion
          |> testList "Multi Word Completion"

          let detailCompletion (contextName, expectedDetail) =
              testCase $"Context \"{contextName}\" has detail \"{expectedDetail}\""
              <| fun () ->
                  let finder: Finder =
                      GlossaryHelper.mockTermNamesFinder
                          { Context.Default with
                              Name = contextName }
                          [ "term" ]

                  let completionItem =
                      (Completion.handler finder Completion.emptyTokenFinder Completion.defaultParams null null)
                          .Result
                      |> Seq.head

                  test <@ completionItem.Detail = expectedDetail @>

          [ (null, null); ("context name", "context name Context") ]
          |> List.map detailCompletion
          |> testList "Detail Completion"

          let documentationCompletion (termName, termDefinition) =
              testCase $"Context \"{termName}\": \"{termDefinition}\" has expected documentation"
              <| fun () ->
                  let terms =
                      [ { Term.Default with
                            Name = termName
                            Definition = termDefinition } ]

                  let finder: Finder = GlossaryHelper.mockDefinitionsFinder Context.Default terms

                  let completionItem =
                      (Completion.handler finder Completion.emptyTokenFinder Completion.defaultParams null null)
                          .Result
                      |> Seq.head

                  let expectedDocumentation = Rendering.renderTerm terms
                  test <@ completionItem.Documentation.HasMarkupContent @>
                  test <@ completionItem.Documentation.MarkupContent.Value = expectedDocumentation.Value @>

          [ ("term", None); ("term", Some "some Definition") ]
          |> List.map documentationCompletion
          |> testList "Documentation Completion"


          testCase "Completion Kind Is Reference"
          <| fun () ->
              let finder: Finder =
                  GlossaryHelper.mockDefinitionsFinder Context.Default [ Term.Default ]

              let completionItem =
                  (Completion.handler finder Completion.emptyTokenFinder Completion.defaultParams null null)
                      .Result
                  |> Seq.head

              test <@ completionItem.Kind = CompletionItemKind.Reference @> ]

let private testCompletionsWithLargeGlossaries (text: string, position, expectedLength) =
    testAsync
        $"""Given content '{text}' at position {position} respond with expected completion list count {expectedLength}""" {

        let workspaceRelative = Path.Combine("fixtures", "performance")

        let config = [ Workspace.optionsBuilder <| workspaceRelative ]

        let workspaceBase =
            Workspace.workspaceFolderPath workspaceRelative |> _.ToUri().ToString()

        use! client = TestClient config |> init

        let textDocumentUri = Path.Combine(workspaceBase, "test.txt")

        let sw = System.Diagnostics.Stopwatch()

        sw.Start()
        let! result = client |> Completion.getCompletionFromText text textDocumentUri position
        sw.Stop()

        let completionLabels = Completion.getLabels result

        test <@ Seq.length completionLabels = expectedLength @>
        test <@ sw.Elapsed < System.TimeSpan.FromSeconds 5 @>
    }

[<Tests>]
let performanceTests =
    [ "", Position(0, 0), 60 * 4 * 16 // 15 terms x 4 variations per term * 16 glossaries
      "disguised_stinger", Position(0, 11), 2 ] // 1 term * 2 variations (we always return at least 2 variations and let the client do further filtering)
    |> List.map testCompletionsWithLargeGlossaries
    |> testList "LanguageServer Performance.Test Completions with large glossaries"
    |> testSequenced
