module Contextive.LanguageServer.Tests.NSubGlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open System.Linq
open Helpers.SubGlossaryHelper

module CA = Helpers.ConditionAwaiter

[<Tests>]
let tests =

    let emptyGlossary =
        """contexts:
  - terms:"""

    testList
        "LanguageServer.SubGlossary Tests"
        [ testAsync "When starting a subglossary it should read the file at the provided path" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p
                  Ok(emptyGlossary)

              let _ = NSubGlossary.create fileReader "path1"

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When reloading a subglossary it should read the file at the path provided when it was created" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p
                  Ok(emptyGlossary)

              let subGlossary = NSubGlossary.create fileReader "path1"

              do! CA.expectMessage awaiter "path1"

              CA.clear awaiter

              NSubGlossary.reload subGlossary

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When looking up a term in a subglossary it should return terms" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p

                  """contexts:
  - terms:
    - name: subGlossary1"""
                  |> Ok

              let subGlossary = NSubGlossary.create fileReader "path1"

              let! result = NSubGlossary.lookup subGlossary id

              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "subGlossary1" @>
          }

          testAsync "SubGlossary can recover from FileReading Failure" {
              let awaiter = CA.create ()

              let ErrorFileResult = Error(Contextive.Core.File.FileError.FileNotFound)

              let OkFileResult =
                  """contexts:
  - terms:
    - name: subGlossary1"""
                  |> Ok

              let mutable fileResult = ErrorFileResult

              let fileReader p =
                  CA.received awaiter p
                  fileResult

              let subGlossary = NSubGlossary.create fileReader "path1"

              let! result = NSubGlossary.lookup subGlossary id

              test <@ result.Count() = 0 @>

              fileResult <- OkFileResult
              NSubGlossary.reload subGlossary

              let! result = NSubGlossary.lookup subGlossary id
              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "subGlossary1" @>
          }

          ]
