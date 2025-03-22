module Contextive.LanguageServer.Tests.Component.RemoteFileReaderTests

open Expecto
open Contextive.Core.File
open Contextive.LanguageServer
open System.IO

let t = Swensen.Unquote.Assertions.test

[<Tests>]
let tests =
    testList
        "LanguageServer.Remote File Reader Tests"
        [

          test "Given valid URL Remote reader returns payload" {
              let result = RemoteFileReader.read "https://httpstat.us/200"

              match result with
              | Error e -> failtest $"Should not get error, got {e}"
              | Ok(m) -> t <@ m = "200 OK" @>
          }

          test "Given not found URL Remote reader returns Error" {
              let result = RemoteFileReader.read "https://httpstat.us/404"

              match result with
              | Error e -> t <@ e = FileNotFound @>
              | _ -> failtest "Should not find content at this url"
          }

          test "Given unknown domain, should report error" {
              let result = RemoteFileReader.read "https://httpstat.us.not.a.domain"

              match result with
              | Error(ReadingError e) -> t <@ e = "Name or service not known (httpstat.us.not.a.domain:443)" @>
              | _ -> failtest "Should not find content at this url"
          }

          test "Given other status codes, should report error" {
              let result = RemoteFileReader.read "https://httpstat.us/401"

              match result with
              | Error(ReadingError e) -> t <@ e = "HttpStatusCode:401, payload: '401 Unauthorized'" @>
              | _ -> failtest "Should not find content at this url"
          }


          ]
