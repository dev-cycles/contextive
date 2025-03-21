module Contextive.LanguageServer.Tests.Component.SubGlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open System.Linq
open Tests.Helpers.SubGlossaryHelper
open Contextive.LanguageServer.Logger
open Contextive.Core.File
open Contextive.LanguageServer.Tests.Helpers

module CA = Tests.Helpers.ConditionAwaiter

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
              "Import"
              [ testAsync "When a subglossary has an import statement it should import the referenced file" {
                    let awaiter = CA.create ()

                    let fileToImport = "../../fileToImport.yml"
                    let initialFilePath = "path1"

                    let glossaryWithImport =
                        $"\
imports:
  - {fileToImport}
"

                    let fileReader p =
                        if p.Path = initialFilePath then
                            Ok glossaryWithImport
                        elif p.Path = fileToImport then
                            CA.received awaiter p.Path
                            Ok emptyGlossary
                        else
                            Error FileError.FileNotFound

                    let _ = pc initialFilePath |> newStartSubGlossary |> SubGlossary.start fileReader

                    do! CA.expectMessage awaiter fileToImport
                }

                testAsync
                    "When a subglossary has an import statement it should merge imported contexts with existing contexts" {
                    let fileToImport = "/fileToImport.yml"
                    let initialFilePath = "/path1"

                    let importedGlossary =
                        $"\
contexts:
  - terms:
    - name: importedTerm"

                    let glossaryWithImport =
                        $"\
imports:
  - {fileToImport}
contexts:
  - terms:
    - name: originalTerm
"

                    let fileReader p =
                        if p.Path = initialFilePath then Ok glossaryWithImport
                        elif p.Path = fileToImport then Ok importedGlossary
                        else Error FileError.FileNotFound

                    let subGlossary =
                        pc initialFilePath |> newStartSubGlossary |> SubGlossary.start fileReader

                    let! result = SubGlossary.lookup subGlossary "" id

                    let terms = FindResult.allTerms result

                    test <@ result.Count() = 2 @>
                    test <@ terms |> Seq.map _.Name |> Set.ofSeq = Set.ofList [ "importedTerm"; "originalTerm" ] @>
                }

                testAsync "When an imported subglossary has an import statement it should chain the imports" {
                    let fileToImport n = $"/fileToImport{n}.yml"

                    let importedGlossary n =
                        $"\
contexts:
  - terms:
    - name: term{n}"

                    let glossaryWithImport n =
                        $"\
imports:
  - {fileToImport <| n + 1}
contexts:
  - terms:
    - name: term{n}"

                    let fileReader p =
                        if p.Path = fileToImport 1 then Ok <| glossaryWithImport 1
                        elif p.Path = fileToImport 2 then Ok <| glossaryWithImport 2
                        elif p.Path = fileToImport 3 then Ok <| importedGlossary 3
                        else Error FileNotFound

                    let subGlossary =
                        1 |> fileToImport |> pc |> newStartSubGlossary |> SubGlossary.start fileReader

                    let! result = SubGlossary.lookup subGlossary "" id

                    let terms = FindResult.allTerms result

                    test <@ result.Count() = 3 @>

                    test <@ terms |> Seq.map _.Name |> Set.ofSeq = Set.ofList [ "term1"; "term2"; "term3" ] @>
                }

                testAsync "Circular imports should be blocked" {
                    let fileToImport n = $"/fileToImport{n}.yml"

                    let glossaryWithImport n =
                        $"\
imports:
  - {fileToImport n}
contexts:
  - terms:
    - name: term{n}"

                    let fileReader p =
                        if p.Path = fileToImport 1 then Ok <| glossaryWithImport 2
                        elif p.Path = fileToImport 2 then Ok <| glossaryWithImport 1
                        else Error FileNotFound

                    let subGlossary =
                        1 |> fileToImport |> pc |> newStartSubGlossary |> SubGlossary.start fileReader

                    let! result = SubGlossary.lookup subGlossary "" id

                    let terms = FindResult.allTerms result

                    test <@ result.Count() = 2 @>

                    test <@ terms |> Seq.map _.Name |> Set.ofSeq = Set.ofList [ "term1"; "term2" ] @>
                }

                ]


          testList
              "Integration"
              [ testAsync "SubGlossary can collaborate with FileReader" {
                    let subGlossary =
                        pc Fixtures.One.path
                        |> newStartSubGlossary
                        |> SubGlossary.start FileReader.pathReader

                    let! result = SubGlossary.lookup subGlossary "" id

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ (foundNames, Fixtures.One.expectedTerms) ||> compareList = 0 @>

                }

                testAsync "SubGlossary can collaborate with FileReader with imports" {
                    let subGlossary =
                        pc Fixtures.Imports.Main.path
                        |> newStartSubGlossary
                        |> SubGlossary.start FileReader.pathReader

                    let! result = SubGlossary.lookup subGlossary "" id

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ Set.ofSeq foundNames = Set.ofList [ "main"; "imported" ] @>

                }

                ]

          ]
