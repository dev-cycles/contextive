module Contextly.VsCodeExtension.Tests.InvalidSchema

open Fable.Mocha
open Fable.Import.VSCode
open Helpers

let tests =
    testList "Invalid Schema Tests" [
        testCaseAsync "Should recover from an invalid schema" <| async {
            do! updateConfig ".contextly/invalid_schema.yml"

            let testDocPath = "../test/fixtures/simple_workspace/test.txt"
            let position = vscode.Position.Create(0.0, 10.0)

            do! Completion.expectCompletion testDocPath position <| seq {"some";"text"} <| None

            do! resetConfig()
        
            do! Completion.expectCompletion testDocPath position Completion.DefaultExpectedTerms None
        }
    ]