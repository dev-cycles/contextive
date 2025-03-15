module Contextive.LanguageServer.Tests.SubGlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open System.Linq
open Helpers.SubGlossaryHelper
open Contextive.LanguageServer.Logger
open Contextive.Core.File

module CA = Helpers.ConditionAwaiter

[<Tests>]
let tests =

    let emptyGlossary =
        """contexts:
  - terms:"""

    let getName (t: Contextive.Core.GlossaryFile.Term) = t.Name
    let compareList = Seq.compareWith compare

    let newStartSubGlossary path : SubGlossary.StartSubGlossary = { Path = path; Log = Logger.Noop }

    let pc p : PathConfiguration = { Path = p; IsDefault = false }


    testList
        "LanguageServer.SubGlossary Tests"
        [ testAsync "When starting a subglossary it should read the file at the provided path" {
              let awaiter = CA.create ()

              let fileReader (p: PathConfiguration) =
                  CA.received awaiter p.Path
                  Ok(emptyGlossary)


              let _ = pc "path1" |> newStartSubGlossary |> SubGlossary.start fileReader

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When starting a subglossary it should log the fact that it's loading the path" {
              let awaiter = CA.create ()

              let fileReader _ = Ok emptyGlossary

              { SubGlossary.StartSubGlossary.Path = pc "path1"
                SubGlossary.StartSubGlossary.Log =
                  { info = CA.received awaiter
                    error = fun _ -> () } }
              |> SubGlossary.start fileReader
              |> ignore


              do! CA.expectMessage awaiter "Loading contextive from path1..."
          }

          testAsync "When if reading the file fails, it should log the error" {
              let awaiter = CA.create ()

              let fileReader _ =
                  Error(FileError.ParsingError "parsing error")

              let _ =
                  SubGlossary.start fileReader
                  <| { Path = pc "path1"
                       Log =
                         { info = fun _ -> ()
                           error = CA.received awaiter } }

              do! CA.expectMessage awaiter "Error loading glossary: Parsing Error: parsing error"
          }

          testAsync "When reloading a subglossary it should read the file at the path provided when it was created" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p.Path
                  Ok(emptyGlossary)

              let subGlossary = pc "path1" |> newStartSubGlossary |> SubGlossary.start fileReader

              do! CA.expectMessage awaiter "path1"

              CA.clear awaiter

              SubGlossary.reload subGlossary

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When looking up a term in a subglossary it should return terms" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p.Path

                  """contexts:
  - terms:
    - name: subGlossary1"""
                  |> Ok

              let subGlossary = pc "path1" |> newStartSubGlossary |> SubGlossary.start fileReader

              let! result = SubGlossary.lookup subGlossary "" id

              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "subGlossary1" @>
          }

          testAsync "SubGlossary can recover from FileReading Failure" {
              let awaiter = CA.create ()

              let ErrorFileResult = Error(FileError.FileNotFound)

              let OkFileResult =
                  """contexts:
  - terms:
    - name: subGlossary1"""
                  |> Ok

              let mutable fileResult = ErrorFileResult

              let fileReader p =
                  CA.received awaiter p.Path
                  fileResult

              let subGlossary = pc "path1" |> newStartSubGlossary |> SubGlossary.start fileReader

              let! result = SubGlossary.lookup subGlossary "" id

              test <@ result.Count() = 0 @>

              fileResult <- OkFileResult
              SubGlossary.reload subGlossary

              let! result = SubGlossary.lookup subGlossary "" id
              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "subGlossary1" @>
          }

          testAsync "SubGlossary can recover from FileReading Exception" {
              let ErrorFileResult = Error(FileError.NotYetLoaded)

              let OkFileResult =
                  """contexts:
  - terms:
    - name: subGlossary1"""
                  |> Ok

              let mutable fileResult = ErrorFileResult

              let fileReader p =
                  if fileResult.IsError then
                      System.Exception("Unexpected error") |> raise
                  else
                      fileResult

              let subGlossary = pc "path1" |> newStartSubGlossary |> SubGlossary.start fileReader

              let! result = SubGlossary.lookup subGlossary "" id

              test <@ result.Count() = 0 @>

              fileResult <- OkFileResult
              SubGlossary.reload subGlossary

              let! result = SubGlossary.lookup subGlossary "" id
              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "subGlossary1" @>
          }

          testList
              "Integration"
              [ testAsync "SubGlossary can collaborate with FileReader" {
                    let subGlossary =
                        pc Helpers.Fixtures.One.path
                        |> newStartSubGlossary
                        |> SubGlossary.start FileReader.pathReader

                    let! result = SubGlossary.lookup subGlossary "" id

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ (foundNames, Helpers.Fixtures.One.expectedTerms) ||> compareList = 0 @>

                } ]

          ]
