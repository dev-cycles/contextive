module Contextly.LanguageServer.Tests.DefinitionsTests

open Expecto
open Swensen.Unquote
open System.IO
open Contextly.LanguageServer

[<Tests>]
let definitionsTests =
    testList "Definitions File Tests" [

        let getTermsFromFile() = async {
            let path = Path.Combine("fixtures", "completion_tests", "one.yml") |> Some
            let workspaceFolder = Some ""
            let definitions = Definitions.create()
            Definitions.init definitions (fun _ -> ()) (fun _ -> async.Return path)
            Definitions.addFolder definitions workspaceFolder
            let! fullPath = Definitions.load definitions

            test <@ fullPath.IsSome @>
            test <@ fullPath.Value = "fixtures/completion_tests/one.yml" @>

            return Definitions.find definitions (fun _ -> true)
        }

        let compareList = Seq.compareWith compare

        testAsync "Can load term Names" {
            let! terms = getTermsFromFile()
            let foundNames = terms |> Seq.map (fun t -> t.Name)
            let expectedNames = seq ["firstTerm"; "secondTerm"; "thirdTerm"]
            test <@ (foundNames, expectedNames) ||> compareList = 0 @>
        }

        testAsync "Can load term Definitions" {
            let! terms = getTermsFromFile()
            let foundDefinitions = terms |> Seq.map (fun t -> t.Definition) |> Seq.choose id
            let expectedDefinitions = seq ["The first term in our definitions list"; "The second term in our definitions list"]
            test <@ (foundDefinitions, expectedDefinitions) ||> compareList = 0 @>
        }

        testAsync "Can load term UsageExamples" {
            let! terms = getTermsFromFile()
            let foundDefinitions = terms |> Seq.map (fun t -> t.Examples) |> Seq.filter ((<>) null) |> Seq.map Seq.cast
            let expectedDefinitions = seq [seq ["An arbitrary usage of secondTerm";"Don't forget to secondTerm the firstTerms"]]
            test <@ (foundDefinitions, expectedDefinitions) ||> Seq.compareWith compareList = 0 @>
        }

    ]