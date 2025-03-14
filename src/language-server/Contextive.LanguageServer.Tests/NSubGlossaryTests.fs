module Contextive.LanguageServer.Tests.NSubGlossaryTests

open Expecto
open Swensen.Unquote
open System.IO
open Contextive.LanguageServer
open Helpers.TestClient
open Contextive.Core
open Contextive.Core.File

module CA = Contextive.LanguageServer.Tests.Helpers.ConditionAwaiter

[<Tests>]
let tests =
    testList
        "LanguageServer.SubGlossary Tests"
        [ testAsync "When starting a subglossary it should read the file at the provided path" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p
                  "contexts:"

              let subGlossary = NSubGlossary.create fileReader "path1"

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When reloading a subglossary it should read the file at the path provided when it was created" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p
                  "contexts:"


              let subGlossary = NSubGlossary.create fileReader "path1"

              do! CA.expectMessage awaiter "path1"

              CA.clear awaiter

              NSubGlossary.reload subGlossary

              do! CA.expectMessage awaiter "path1"
          }

          ]
