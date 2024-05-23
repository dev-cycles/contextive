module Contextive.LanguageServer.Tests.FileLoaderTests

open Expecto
open Contextive.Core.File
open Contextive.LanguageServer
open Swensen.Unquote
open System.IO

[<Tests>]
let tests =
    testList
        "LanguageServer.File Loader Tests"
        [

          testAsync "Path is in error state" {
              let pathGetter () = async.Return <| Error("No path")
              let! file = (FileLoader.loader pathGetter) ()
              test <@ file = Error(PathInvalid("No path")) @>
          }

          testAsync "Path Doesn't exist" {
              let path = "/file/not/found"
              let pathGetter () = async.Return <| Ok(path)
              let! file = (FileLoader.loader pathGetter) ()

              match file with
              | Error(e) -> failtest <| e.ToString()
              | Ok(f) ->
                  test <@ f.AbsolutePath = path @>
                  test <@ f.Contents = Error(FileNotFound) @>
          }

          testAsync "Path exists" {
              let path =
                  Path.Combine(Directory.GetCurrentDirectory(), "fixtures/completion_tests/two.yml")

              let pathGetter () = async.Return <| Ok(path)
              let! file = (FileLoader.loader pathGetter) ()

              match file with
              | Error(e) -> failtest <| e.ToString()
              | Ok(f) ->
                  test <@ f.AbsolutePath = path @>

                  test
                      <@
                          f.Contents = Ok(
                              "contexts:
  - terms:
    - name: word1
    - name: word2
    - name: word3"
                          )
                      @>
          } ]
