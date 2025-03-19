module Contextive.LanguageServer.Tests.Component.FileScannerTests

open Expecto
open Contextive.LanguageServer
open Swensen.Unquote
open System.IO
open Contextive.LanguageServer.Tests.Helpers.Workspace

[<Tests>]
let tests =
    testList
        "File Scanner Tests"
        [ testAsync "When scanning folder, find matching files" {
              let basePath = workspaceFolderPath "fixtures/scanning_tests" |> _.ToUri().LocalPath

              let scanner = FileScanner.fileScanner basePath
              let files = scanner "**/*.glossary.yml"

              let expectedFiles =
                  [ "root.glossary.yml"
                    "folder1/folder1.glossary.yml"
                    "folder1/nestedFolder/nested.glossary.yml"
                    "folder2/folder2.glossary.yml" ]
                  |> Seq.map (fun p -> Path.Combine(basePath, p))

              test <@ Set.ofSeq expectedFiles = Set.ofSeq files @>
          } ]
