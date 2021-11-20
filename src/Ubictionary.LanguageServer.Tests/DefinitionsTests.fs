module Ubictionary.LanguageServer.Tests.DefinitionsTests

open Expecto
open Swensen.Unquote
open System.IO
open Ubictionary.LanguageServer

[<Tests>]
let definitionsTests =
    testList "Definitions File Tests" [

        testCase "Can load definitions file without an error" <|
            fun () -> 
                let path = Path.Combine("fixtures", "completion_tests", "one.yml") |> Some
                let workspaceFolder = Some ""
                let id = System.Guid.NewGuid().ToString()
                let fullPath = Definitions.load id workspaceFolder path

                test <@ fullPath.IsSome @>
                test <@ fullPath.Value = "fixtures/completion_tests/one.yml" @>

                let foundLabels = Definitions.find id (fun _ -> true) (fun t -> t.Name)
                let expectedLabels = seq ["firstTerm"; "secondTerm"; "thirdTerm"]
                test <@ (foundLabels, expectedLabels) ||> Seq.compareWith compare = 0 @>
    ]