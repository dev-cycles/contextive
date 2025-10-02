module Contextive.LanguageServer.Tests.Component.LocalFileReaderTests

open Expecto
open Contextive.Core.File
open Contextive.LanguageServer
open System.IO

let t = Swensen.Unquote.Assertions.test

[<Tests>]
let tests =
    testList
        "LanguageServer.Local File Reader Tests"
        [

          test "Path Doesn't exist" {
              let file =
                  LocalFileReader.read
                      { Path = "/file/not/found"
                        Source = Configured }

              match file with
              | Error e -> t <@ e = FileNotFound Configured @>
              | Ok _ -> failtest "Shouldn't received content"
          }

          test "Path exists" {
              let path =
                  Path.Combine(Directory.GetCurrentDirectory(), "fixtures/completion_tests/two.yml")

              let file = LocalFileReader.read { Path = path; Source = Configured }

              match file with
              | Error e -> failtest <| e.ToString()
              | Ok c ->
                  t
                      <@
                          c = "contexts:
  - terms:
      - name: word1
      - name: word2
      - name: word3
"
                      @>
          } ]
