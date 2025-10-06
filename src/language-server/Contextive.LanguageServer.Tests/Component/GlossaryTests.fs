module Contextive.LanguageServer.Tests.Component.GlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open System.Linq
open Tests.Helpers.GlossaryHelper
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

    let newStartGlossary path : Glossary.StartGlossary = { Path = path; Log = Logger.Noop }

    let pc p : PathConfiguration = { Path = p; Source = Configured }


    testList
        "LanguageServer.Glossary Tests"
        [ testAsync "When starting a glossary it should read the file at the provided path" {
              let awaiter = CA.create ()

              let fileReader (p: PathConfiguration) =
                  CA.received awaiter p.Path
                  Ok(emptyGlossary)


              let _ = pc "path1" |> newStartGlossary |> Glossary.start fileReader

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When starting a glossary it should log the fact that it's loading the path" {
              let awaiter = CA.create ()

              let fileReader _ = Ok emptyGlossary

              { Glossary.StartGlossary.Path = pc "path1"
                Glossary.StartGlossary.Log =
                  { Logger.Noop with
                      info = CA.received awaiter } }
              |> Glossary.start fileReader
              |> ignore


              do! CA.expectMessage awaiter "Loading contextive from path1..."
          }

          testAsync "When if reading the file fails, it should log the error" {
              let awaiter = CA.create ()

              let fileReader _ = Error(ParsingError "parsing error.")

              let _ =
                  Glossary.start fileReader
                  <| { Path = pc "path1"
                       Log =
                         { Logger.Noop with
                             error = CA.received awaiter } }

              do! CA.expectMessage awaiter $"Error loading glossary file 'path1': Parsing Error: parsing error."
          }

          testAsync "When reloading a glossary it should read the file at the path provided when it was created" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p.Path
                  Ok(emptyGlossary)

              let glossary = pc "path1" |> newStartGlossary |> Glossary.start fileReader

              do! CA.expectMessage awaiter "path1"

              CA.clear awaiter

              Glossary.reload glossary

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When looking up a term in a glossary it should return terms" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p.Path

                  """contexts:
  - terms:
    - name: glossary1"""
                  |> Ok

              let glossary = pc "path1" |> newStartGlossary |> Glossary.start fileReader

              let! result = Glossary.lookup glossary "" id

              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "glossary1" @>
          }

          testAsync "Glossary can recover from FileReading Failure" {
              let awaiter = CA.create ()

              let ErrorFileResult = FileNotFound Configured |> Error

              let OkFileResult =
                  """contexts:
  - terms:
    - name: glossary1"""
                  |> Ok

              let mutable fileResult = ErrorFileResult

              let fileReader p =
                  CA.received awaiter p.Path
                  fileResult

              let glossary = pc "path1" |> newStartGlossary |> Glossary.start fileReader

              let! result = Glossary.lookup glossary "" id

              test <@ result.Count() = 0 @>

              fileResult <- OkFileResult
              Glossary.reload glossary

              let! result = Glossary.lookup glossary "" id
              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "glossary1" @>
          }

          testAsync "Glossary can recover from FileReading Exception" {
              let ErrorFileResult = Error NotYetLoaded

              let OkFileResult =
                  """contexts:
  - terms:
    - name: glossary1"""
                  |> Ok

              let mutable fileResult = ErrorFileResult

              let fileReader p =
                  if fileResult.IsError then
                      System.Exception "Unexpected error" |> raise
                  else
                      fileResult

              let glossary = pc "path1" |> newStartGlossary |> Glossary.start fileReader

              let! result = Glossary.lookup glossary "" id

              test <@ result.Count() = 0 @>

              fileResult <- OkFileResult
              Glossary.reload glossary

              let! result = Glossary.lookup glossary "" id
              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "glossary1" @>
          }

          testList
              "Import"
              [ testAsync "When a glossary has an import statement it should import the referenced file" {
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
                            FileNotFound Configured |> Error

                    let _ = pc initialFilePath |> newStartGlossary |> Glossary.start fileReader

                    do! CA.expectMessage awaiter fileToImport
                }

                testAsync
                    "When a glossary has an import statement it should merge imported contexts with existing contexts" {
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
                        else FileNotFound Configured |> Error

                    let glossary = pc initialFilePath |> newStartGlossary |> Glossary.start fileReader

                    let! result = Glossary.lookup glossary "/" id

                    let terms = FindResult.allTerms result

                    test <@ result.Count() = 2 @>
                    test <@ terms |> Seq.map _.Name |> Set.ofSeq = Set.ofList [ "importedTerm"; "originalTerm" ] @>
                }

                testAsync "When an imported glossary has an import statement it should chain the imports" {
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
                        else FileNotFound Configured |> Error

                    let glossary =
                        1 |> fileToImport |> pc |> newStartGlossary |> Glossary.start fileReader

                    let! result = Glossary.lookup glossary "/" id

                    let terms = FindResult.allTerms result

                    test <@ result.Count() = 3 @>

                    test <@ terms |> Seq.map _.Name |> Set.ofSeq = Set.ofList [ "term1"; "term2"; "term3" ] @>
                }

                testAsync "Can import multiple files" {
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
  - {fileToImport <| n + 2}
contexts:
  - terms:
    - name: term{n}"

                    let fileReader p =
                        if p.Path = fileToImport 1 then Ok <| glossaryWithImport 1
                        elif p.Path = fileToImport 2 then Ok <| importedGlossary 2
                        elif p.Path = fileToImport 3 then Ok <| importedGlossary 3
                        else FileNotFound Configured |> Error

                    let glossary =
                        1 |> fileToImport |> pc |> newStartGlossary |> Glossary.start fileReader

                    let! result = Glossary.lookup glossary "/" id

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
                        else FileNotFound Configured |> Error

                    let glossary =
                        1 |> fileToImport |> pc |> newStartGlossary |> Glossary.start fileReader

                    let! result = Glossary.lookup glossary "/" id

                    let terms = FindResult.allTerms result

                    test <@ result.Count() = 2 @>

                    test <@ terms |> Seq.map _.Name |> Set.ofSeq = Set.ofList [ "term1"; "term2" ] @>
                }

                ]


          testList
              "Integration"
              [ testAsync "Glossary can collaborate with FileReader" {
                    let glossary =
                        pc Fixtures.One.path
                        |> newStartGlossary
                        |> Glossary.start FileReader.configuredReader

                    let! result = Glossary.lookup glossary "/" id

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ (foundNames, Fixtures.One.expectedTerms) ||> compareList = 0 @>

                }

                testAsync "Glossary can collaborate with FileReader with imports" {
                    let glossary =
                        pc Fixtures.Imports.Main.path
                        |> newStartGlossary
                        |> Glossary.start FileReader.configuredReader

                    let! result = Glossary.lookup glossary "/" id

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ Set.ofSeq foundNames = Set.ofList [ "main"; "imported" ] @>

                }

                testAsync "Glossary can collaborate with FileReader with remote imports" {
                    let glossary =
                        pc Fixtures.RemoteImports.path
                        |> newStartGlossary
                        |> Glossary.start FileReader.configuredReader

                    let! result = Glossary.lookup glossary "/" id

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ Set.ofSeq foundNames = Set.ofList [ "imported" ] @>
                }

                ]

          ]
