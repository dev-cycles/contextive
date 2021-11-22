module Ubictionary.LanguageServer.Tests.DefinitionsTests

open Expecto
open Swensen.Unquote
open System.IO
open Ubictionary.LanguageServer

[<Tests>]
let definitionsTests =
    testList "Definitions File Tests" [

        let getTermsFromFile =
            let path = Path.Combine("fixtures", "completion_tests", "one.yml") |> Some
            let workspaceFolder = Some ""
            let runId = System.Guid.NewGuid().ToString()
            let fullPath = Definitions.load runId workspaceFolder path

            test <@ fullPath.IsSome @>
            test <@ fullPath.Value = "fixtures/completion_tests/one.yml" @>

            Definitions.find runId (fun _ -> true)

        let compareList = Seq.compareWith compare

        testCase "Can load term Names" <|
            fun () -> 
                let foundNames = getTermsFromFile |> Seq.map (fun t -> t.Name)
                let expectedNames = seq ["firstTerm"; "secondTerm"; "thirdTerm"]
                test <@ (foundNames, expectedNames) ||> compareList = 0 @>

        testCase "Can load term Definitions" <|
            fun () -> 
                let foundDefinitions = getTermsFromFile |> Seq.map (fun t -> t.Definition) |> Seq.choose id
                let expectedDefinitions = seq ["The first term in our definitions list"; "The second term in our definitions list"]
                test <@ (foundDefinitions, expectedDefinitions) ||> compareList = 0 @>

        testCase "Can load term UsageExamples" <|
            fun () -> 
                let foundDefinitions = getTermsFromFile |> Seq.map (fun t -> t.Examples) |> Seq.filter ((<>) null) |> Seq.map Seq.cast
                let expectedDefinitions = seq [seq ["An arbitrary usage of secondTerm";"Don't forget to secondTerm the firstTerms"]]
                test <@ (foundDefinitions, expectedDefinitions) ||> Seq.compareWith compareList = 0 @>

    ]