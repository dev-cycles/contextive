module Contextive.LanguageServer.Tests.FileScannerTests

open Expecto
open Contextive.Core.File
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
              let files = scanner ()

              let expectedFiles =
                  [ "root.contextive.yml"
                    "folder1/folder1.contextive.yml"
                    "folder1/nestedFolder/nested.contextive.yml"
                    "folder2/folder2.contextive.yml" ]
                  |> Seq.map (fun p -> Path.Combine(basePath, p))

              test <@ Set.ofSeq expectedFiles = Set.ofSeq files @>
          } ]
