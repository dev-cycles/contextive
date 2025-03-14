module Contextive.LanguageServer.Tests.NSubGlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open System.Linq
open Helpers.SubGlossaryHelper
open Contextive.LanguageServer.Logger

module CA = Helpers.ConditionAwaiter

[<Tests>]
let tests =

    let emptyGlossary =
        """contexts:
  - terms:"""

    let getName (t: Contextive.Core.GlossaryFile.Term) = t.Name
    let compareList = Seq.compareWith compare

    let newStartSubGlossary path : NSubGlossary.StartSubGlossary = { Path = path; Log = Logger.Noop }


    testList
        "LanguageServer.SubGlossary Tests"
        [ testAsync "When starting a subglossary it should read the file at the provided path" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p
                  Ok(emptyGlossary)

              let _ = NSubGlossary.start fileReader <| newStartSubGlossary "path1"

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When starting a subglossary it should log the fact that it's loading the path" {
              let awaiter = CA.create ()

              let fileReader _ = Ok emptyGlossary

              { NSubGlossary.StartSubGlossary.Path = "path1"
                NSubGlossary.StartSubGlossary.Log = { info = CA.received awaiter } }
              |> NSubGlossary.start fileReader
              |> ignore


              do! CA.expectMessage awaiter $"Loading contextive from path1..."
          }

          testAsync "When reloading a subglossary it should read the file at the path provided when it was created" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p
                  Ok(emptyGlossary)

              let subGlossary = NSubGlossary.start fileReader <| newStartSubGlossary "path1"

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

              let subGlossary = NSubGlossary.start fileReader <| newStartSubGlossary "path1"

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

              let subGlossary = NSubGlossary.start fileReader <| newStartSubGlossary "path1"

              let! result = NSubGlossary.lookup subGlossary id

              test <@ result.Count() = 0 @>

              fileResult <- OkFileResult
              NSubGlossary.reload subGlossary

              let! result = NSubGlossary.lookup subGlossary id
              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "subGlossary1" @>
          }

          testList
              "Integration"
              [ testAsync "SubGlossary can collaborate with FileReader" {
                    let subGlossary =
                        NSubGlossary.start FileReader.pathReader
                        <| newStartSubGlossary Helpers.Fixtures.One.path

                    let! result = NSubGlossary.lookup subGlossary id

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ (foundNames, Helpers.Fixtures.One.expectedTerms) ||> compareList = 0 @>

                } ]

          ]
