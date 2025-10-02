module Contextive.LanguageServer.Tests.Component.RemoteFileReaderTests

open Expecto
open Contextive.Core.File
open Contextive.LanguageServer
open System.IO

let t = Swensen.Unquote.Assertions.test

let mockStatusUrl = FileReaderTests.mockStatusUrl

[<Tests>]
let tests =
    testList
        "LanguageServer.Remote File Reader Tests"
        [

          test "Given valid URL Remote reader returns payload" {
              let result =
                  RemoteFileReader.read
                      { Path = $"{mockStatusUrl}200"
                        Source = Imported }

              match result with
              | Error e -> failtest $"Should not get error, got {e}"
              | Ok(m) -> t <@ m = "200 OK" @>
          }

          test "Given not found URL Remote reader returns Error" {
              let result =
                  RemoteFileReader.read
                      { Path = $"{mockStatusUrl}404"
                        Source = Imported }

              match result with
              | Error e -> t <@ e = FileNotFound Imported @>
              | _ -> failtest "Should not find content at this url"
          }

          test "Given unknown domain, should report error" {
              let result =
                  RemoteFileReader.read
                      { Path = "https://httpstat.us.not.a.domain"
                        Source = Imported }

              //macos: nodename nor servname provided, or not known (httpstat.us.not.a.domain:443)
              //win: No such host is known. (httpstat.us.not.a.domain:443)
              //linux: Name or service not known (httpstat.us.not.a.domain:443)
              match result with
              | Error(ReadingError e) -> t <@ e.Contains "known" && e.Contains "(httpstat.us.not.a.domain:443)" @>
              | _ -> failtest "Should not find content at this url"
          }

          test "Given other status codes, should report error" {
              let result = RemoteFileReader.read {Path = $"{mockStatusUrl}401"; Source = Imported}

              match result with
              | Error(ReadingError e) -> t <@ e = "HttpStatusCode:401, payload: '401 Unauthorized'" @>
              | _ -> failtest "Should not find content at this url"
          }


          ]
