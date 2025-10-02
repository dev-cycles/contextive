module Contextive.LanguageServer.Tests.Component.FileReaderTests

open Expecto
open Contextive.Core.File
open Contextive.LanguageServer

let t = Swensen.Unquote.Assertions.test

let mockStatusUrl = "https://mock.httpstatus.io/"

let importedPath path = { Path = path; Source = Imported }

[<Tests>]
let tests =
    testList
        "LanguageServer.File Reader Tests"
        [ test "Non-default local path not found" {
              let path = "/file/not/found" |> configuredPath
              let file = FileReader.configuredReader path

              match file with
              | Error e -> t <@ e = FileNotFound Configured @>
              | _ -> failtest "Should not receive Ok"
          }

          test "Default local path not found" {
              let path =
                  { Source = Default
                    Path = "/file/not/found" }

              let file = FileReader.configuredReader path

              match file with
              | Error e -> t <@ e = FileNotFound Default @>
              | _ -> failtest "Should not receive Ok"
          }

          test "Local Path exists" {
              let path =
                  System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "fixtures/completion_tests/two.yml")
                  |> configuredPath


              let file = FileReader.configuredReader path

              match file with
              | Error e -> failtest <| e.ToString()
              | Ok c ->
                  t
                      <@
                          c = "\
contexts:
  - terms:
      - name: word1
      - name: word2
      - name: word3
"
                      @>
          }

          test "Non-default remote path not found" {
              let path = $"{mockStatusUrl}404" |> importedPath
              let file = FileReader.configuredReader path

              match file with
              | Error e -> t <@ e = FileNotFound Imported @>
              | _ -> failtest "Should not receive Ok"
          }

          test "Default remote path not found" {
              let path = importedPath $"{mockStatusUrl}404"

              let file = FileReader.configuredReader path

              match file with
              | Error e -> t <@ e = FileNotFound Imported @>
              | _ -> failtest "Should not receive Ok"
          }

          test "Remote Path exists" {
              let path = $"{mockStatusUrl}200" |> importedPath


              let file = FileReader.configuredReader path

              match file with
              | Error e -> failtest <| e.ToString()
              | Ok c -> t <@ c = "200 OK" @>
          }


          ]
