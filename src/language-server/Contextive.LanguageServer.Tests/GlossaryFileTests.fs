module Contextive.LanguageServer.Tests.GlossaryFileTests

open Expecto
open Swensen.Unquote
open System.IO
open Contextive.LanguageServer
open Helpers.TestClient
open Contextive.Core
open Contextive.Core.File
open Contextive.LanguageServer.Tests.Helpers

[<Tests>]
let tests =
    testList
        "LanguageServer.GlossaryFile Tests"
        [

          let getName (t: GlossaryFile.Term) = t.Name

          let getGlossaryFileWithErrorHandler onErrorAction configGetter workspaceFolder =
              async {
                  let glossaryFile = GlossaryFile.create ()

                  let glossaryFileReader =
                      PathResolver.resolvePath workspaceFolder
                      |> Configuration.resolvedPathGetter configGetter
                      |> FileReader.reader

                  GlossaryFile.init glossaryFile (fun _ -> ()) glossaryFileReader None onErrorAction
                  (GlossaryFile.loader glossaryFile) ()
                  return glossaryFile
              }

          let getGlossaryFile = getGlossaryFileWithErrorHandler (fun _ -> ())

          let getFileName glossaryFileName =
              Path.Combine("fixtures", "completion_tests", $"{glossaryFileName}.yml")
              |> Some

          let getTermsFromFileInContext termFileUri glossaryFileName =
              async {
                  let glossaryFilePath = getFileName glossaryFileName
                  let workspaceFolder = Some ""
                  let configGetter = (fun _ -> async.Return <| Option.map configuredPath glossaryFilePath)
                  let! glossaryFile = getGlossaryFile configGetter workspaceFolder
                  let! contexts = GlossaryFile.find glossaryFile termFileUri id
                  return contexts |> GlossaryFile.FindResult.allTerms
              }

          let getTermsFromFile = getTermsFromFileInContext ""

          let compareList = Seq.compareWith compare

          testAsync "Can load term Names" {
              let! terms = getTermsFromFile "one"
              let foundNames = terms |> Seq.map getName
              test <@ (foundNames, Fixtures.One.expectedTerms) ||> compareList = 0 @>
          }

          testAsync "Can load term Definitions" {
              let! terms = getTermsFromFile "one"
              let foundDefinitions = terms |> Seq.map (fun t -> t.Definition) |> Seq.choose id

              let expectedDefinitions =
                  seq
                      [ "The first term in our definitions list"
                        "The second term in our definitions list" ]

              test <@ (foundDefinitions, expectedDefinitions) ||> compareList = 0 @>
          }

          testAsync "Can load term UsageExamples" {
              let! terms = getTermsFromFile "one"

              let foundDefinitions =
                  terms
                  |> Seq.map (fun t -> t.Examples)
                  |> Seq.filter ((<>) null)
                  |> Seq.map Seq.cast

              let expectedDefinitions =
                  seq
                      [ seq
                            [ "An arbitrary usage of secondTerm"
                              "Don't forget to secondTerm the firstTerms" ] ]

              test <@ (foundDefinitions, expectedDefinitions) ||> Seq.compareWith compareList = 0 @>
          }

          testAsync "Can load multi-context definitions" {
              let! terms = getTermsFromFileInContext "/primary/secondary/test.txt" "multi" // Path contains both context's globs
              let foundNames = terms |> Seq.map getName
              test <@ (foundNames, seq [ "termInPrimary"; "termInSecondary" ]) ||> compareList = 0 @>
          }

          let testInPath ((path: string), expectedTerms) =
              let pathName = path.Replace(".", "_dot_")

              testAsync $"with path {pathName}, expecting {expectedTerms}" {
                  let! terms = getTermsFromFileInContext path "multi"
                  let foundNames = terms |> Seq.map getName
                  test <@ (foundNames, seq expectedTerms) ||> compareList = 0 @>
              }

          [ ("/some/path/with/primary/in/it.txt", [ "termInPrimary" ])
            ("/some/path/with/primary.txt", [ "termInPrimary" ])
            ("/some/path/with/primary", [ "termInPrimary" ])
            ("/some/path/with/test.js", [ "termInPrimary" ])
            ("/primary", [ "termInPrimary" ])
            ("/some/path/with/secondary/in/it.txt", [ "termInSecondary" ])
            ("/some/path/with/secondary.txt", [ "termInSecondary" ])
            ("/some/path/with/secondary", [ "termInSecondary" ])
            ("/secondary", [ "termInSecondary" ])
            ("/some/path", [])
            ("/some/path/test.cs", []) ]
          |> List.map testInPath
          |> testList "Can load definition from correct context"

          let invalidScenarios =
              [ ("invalid_empty", "Error loading glossary: Parsing Error: Glossary file is empty.")
                ("invalid_schema",
                 "Error loading glossary: Parsing Error: Object starting line 6, column 9 - Property 'example' not found on type 'Contextive.Core.GlossaryFile+Term'.")
                ("invalid_schema2",
                 "Error loading glossary: Parsing Error: Object starting line 5, column 19 - Mapping values are not allowed in this context.")
                ("no_file", "Error loading glossary: Glossary file not found.") ]

          let canRecoverFromInvalidGlossaryFile (fileName, expectedErrorMessage) =
              testAsync fileName {
                  let mutable path = getFileName fileName

                  let workspaceFolder = Some ""
                  let configGetter = (fun _ -> async.Return <| Option.map configuredPath path)

                  let errorMessageAwaiter = ConditionAwaiter.create ()

                  let onErrorLoading = fun msg -> ConditionAwaiter.received errorMessageAwaiter msg

                  let! glossaryFile = getGlossaryFileWithErrorHandler onErrorLoading configGetter workspaceFolder
                  let! termsWhenInvalid = GlossaryFile.find glossaryFile "" id

                  let! errorMessage = ConditionAwaiter.waitForAny errorMessageAwaiter
                  test <@ errorMessage.Value = expectedErrorMessage @>
                  test <@ Seq.length termsWhenInvalid = 0 @>

                  path <- getFileName "one"
                  (GlossaryFile.loader glossaryFile) ()

                  let! contexts = GlossaryFile.find glossaryFile "" id
                  let foundNames = contexts |> GlossaryFile.FindResult.allTerms |> Seq.map getName

                  test <@ (foundNames, Fixtures.One.expectedTerms) ||> compareList = 0 @>
              }

          invalidScenarios
          |> List.map canRecoverFromInvalidGlossaryFile
          |> testList "Can recover from invalid glossary file"

          let canRecoverFromInvalidGlossaryFileWhenConfigChanges (fileName, _) =
              testAsync fileName {
                  let validPath = "one.yml"
                  let mutable path = validPath

                  let pathLoader () : obj = path

                  let config =
                      [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathLoaderBuilder pathLoader ]

                  let! (client, _, logAwaiter) =
                      TestClientWithCustomInitWait(config, Some "Successfully loaded.")
                      |> initAndWaitForReply

                  use client = client

                  let! termsWhenValidAtStart = Completion.getCompletionLabels client
                  test <@ (termsWhenValidAtStart, Fixtures.One.expectedCompletionLabels) ||> compareList = 0 @>

                  path <- $"{fileName}.yml"
                  do! ConfigurationSection.didChangePath client path logAwaiter

                  let! termsWhenInvalid = Completion.getCompletionLabels client
                  test <@ Seq.length termsWhenInvalid = 0 @>

                  path <- validPath
                  do! ConfigurationSection.didChangePath client path logAwaiter

                  let! termsWhenValidAtEnd = Completion.getCompletionLabels client
                  test <@ (termsWhenValidAtEnd, Fixtures.One.expectedCompletionLabels) ||> compareList = 0 @>
              }

          invalidScenarios
          |> List.map canRecoverFromInvalidGlossaryFileWhenConfigChanges
          |> testList "Can recover from invalid glossary file when config changes" ]
