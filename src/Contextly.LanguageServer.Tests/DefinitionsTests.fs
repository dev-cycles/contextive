module Contextly.LanguageServer.Tests.DefinitionsTests

open Expecto
open Swensen.Unquote
open System.IO
open Contextly.LanguageServer
open TestClient
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol.Models

[<Tests>]
let definitionsTests =
    testList "Definitions File Tests" [

        let getDefinitions configGetter workspaceFolder = async {
            let definitions = Definitions.create()
            Definitions.init definitions (fun _ -> ()) configGetter None (fun _ -> ())
            Definitions.addFolder definitions workspaceFolder
            (Definitions.loader definitions)()
            return definitions
        }

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

        let canRecoverFromInvalidDefinitions fileName = testAsync fileName {
            let mutable path = getFileName fileName
            let workspaceFolder = Some ""
            let configGetter = (fun _ -> async.Return path)

            let! definitions = getDefinitions configGetter workspaceFolder
            let! termsWhenInvalid = Definitions.find definitions (fun _ -> true)

            test <@ Seq.length termsWhenInvalid = 0 @>

            path <- getFileName "one"
            (Definitions.loader definitions)()

            let! termsWhenValid = Definitions.find definitions (fun _ -> true)
            let foundNames = termsWhenValid |> Seq.map (fun t -> t.Name)

            test <@ (foundNames, oneExpectedNames) ||> compareList = 0 @>
        }

        [
            "invalid_empty"
            "invalid_schema"
        ] |> List.map canRecoverFromInvalidDefinitions |> testList "Can recover from invalid definitions"
    ]