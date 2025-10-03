module Contextive.LanguageServer.Tests.Component.FileScannerTests

open Expecto
open Contextive.LanguageServer
open Swensen.Unquote
open System.IO
open Contextive.LanguageServer.Tests.Helpers.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol

let private EXPECTED_GLOSSARY_FILE_GLOB =
    [| "**/*.glossary.yml"; "**/*.glossary.yaml" |]

let private fileScanner = FileScanner.fileScanner ignore

[<Tests>]
let tests =

    let normalizePath (p: DocumentUri) = p.ToUri().LocalPath

    let reBaseLinePaths basePath (paths: string list) =
        paths |> List.map (fun p -> Path.Combine(basePath, p))

    testList
        "LanguageServer.File Scanner Tests"
        [ testAsync "When scanning folder, find matching files" {

              let base1 = workspaceFolderPath "fixtures/scanning_tests" |> normalizePath
              let base2 = workspaceFolderPath "fixtures/scanning_tests2" |> normalizePath
              let basePaths = [ base1; base2 ]

              let scanner = fileScanner basePaths
              let files = scanner EXPECTED_GLOSSARY_FILE_GLOB

              let expectedFiles1 =
                  [ "root.glossary.yml"
                    "folder1/folder1.glossary.yml"
                    "folder1/nestedFolder/nested.glossary.yml"
                    "folder2/folder2.glossary.yaml" ]
                  |> reBaseLinePaths base1

              let expectedFiles2 = [ "extra.glossary.yml" ] |> reBaseLinePaths base2

              let expectedFiles = List.append expectedFiles1 expectedFiles2

              test <@ Set.ofSeq expectedFiles = Set.ofSeq files @>
          }

          testAsync "When scanning folder, find matching files except for .gitignored files" {

              let basePath = workspaceFolderPath "fixtures/scanning_tests_ignore" |> normalizePath

              let scanner = fileScanner [ basePath ]
              let files = scanner EXPECTED_GLOSSARY_FILE_GLOB

              let expectedFiles =
                  [ "test.glossary.yml"; ".nested/shouldfind.glossary.yml" ]
                  |> reBaseLinePaths basePath

              test <@ Set.ofSeq expectedFiles = Set.ofSeq files @>
          } ]
