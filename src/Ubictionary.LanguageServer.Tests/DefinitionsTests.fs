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
                Definitions.load workspaceFolder path |> ignore
                test <@ Definitions.definitions <> None @>
    ]