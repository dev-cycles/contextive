module Contextly.LanguageServer.Tests.DefinitionsTests

open Expecto
open Swensen.Unquote
open System.IO
open Contextly.LanguageServer

[<Tests>]
let definitionsTests =
    testList "Definitions File Tests" [

        let getDefinitionsWithErrorHandler onErrorAction configGetter workspaceFolder  = async {
            let definitions = Definitions.create()
            Definitions.init definitions (fun _ -> ()) configGetter None onErrorAction
            Definitions.addFolder definitions workspaceFolder
            (Definitions.loader definitions)()
            return definitions
        }

        let getDefinitions = getDefinitionsWithErrorHandler (fun _ -> ())

        let getFileName fileName = Path.Combine("fixtures", "completion_tests", $"{fileName}.yml") |> Some

        let getTermsFromFile fileName = async {
            let path = getFileName fileName
            let workspaceFolder = Some ""
            let configGetter = (fun _ -> async.Return path)
            let! definitions = getDefinitions configGetter workspaceFolder
            return! Definitions.find definitions (fun _ -> true)
        }

        let compareList = Seq.compareWith compare

        let oneExpectedNames = seq ["firstTerm"; "secondTerm"; "thirdTerm"]

        testAsync "Can load term Names" {
            let! terms = getTermsFromFile "one"
            let foundNames = terms |> Seq.map (fun t -> t.Name)
            test <@ (foundNames, oneExpectedNames) ||> compareList = 0 @>
        }

        testAsync "Can load term Definitions" {
            let! terms = getTermsFromFile "one"
            let foundDefinitions = terms |> Seq.map (fun t -> t.Definition) |> Seq.choose id
            let expectedDefinitions = seq ["The first term in our definitions list"; "The second term in our definitions list"]
            test <@ (foundDefinitions, expectedDefinitions) ||> compareList = 0 @>
        }

        testAsync "Can load term UsageExamples" {
            let! terms = getTermsFromFile "one"
            let foundDefinitions = terms |> Seq.map (fun t -> t.Examples) |> Seq.filter ((<>) null) |> Seq.map Seq.cast
            let expectedDefinitions = seq [seq ["An arbitrary usage of secondTerm";"Don't forget to secondTerm the firstTerms"]]
            test <@ (foundDefinitions, expectedDefinitions) ||> Seq.compareWith compareList = 0 @>
        }

        let canRecoverFromInvalidDefinitions (fileName, expectedErrorMessage) =
            testAsync fileName {
                let mutable path = getFileName fileName
                let workspaceFolder = Some ""
                let configGetter = (fun _ -> async.Return path)

                let errorMessage = ref ""
                let onErrorLoading = fun msg -> errorMessage.Value <- msg
                let! definitions = getDefinitionsWithErrorHandler onErrorLoading configGetter workspaceFolder
                let! termsWhenInvalid = Definitions.find definitions (fun _ -> true)

                do! Async.Sleep 100

                test <@ errorMessage.Value = expectedErrorMessage @>
                test <@ Seq.length termsWhenInvalid = 0 @>

                path <- getFileName "one"
                (Definitions.loader definitions)()

                let! termsWhenValid = Definitions.find definitions (fun _ -> true)
                let foundNames = termsWhenValid |> Seq.map (fun t -> t.Name)

                test <@ (foundNames, oneExpectedNames) ||> compareList = 0 @>
            }

        [
            ("invalid_empty","Error loading definitions: Definitions file is empty.")
            ("invalid_schema","Error loading definitions: Error parsing definitions file:  Object starting line 4, column 7 - Property 'example' not found on type 'Contextly.LanguageServer.Definitions+Term'.")
        ] |> List.map canRecoverFromInvalidDefinitions |> testList "Can recover from invalid definitions"
    ]