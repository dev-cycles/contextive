module Contextly.VsCodeExtension.Tests.InvalidSchema

open Fable.Mocha
open Fable.Import.VSCode
open Initialize

let tests =
    testList "Invalid Schema Tests" [
        testCaseAsync "Should recover from an invalid schema" <| async {
            let newPath = $".contextly/invalid_schema.yml"
            do! updateConfig newPath

            do! VsCodeCommands.initialize() |> Promise.Ignore
            
            do! Promise.sleep 500

            let resetWorkspaceHook = Some (fun _ -> promise {do! resetConfig()})

            let testDocPath = "../test/fixtures/simple_workspace/test.txt"
            let position = vscode.Position.Create(0.0, 10.0)
            let expectedResults = seq {"some";"text"}
            
            do! Completion.expectCompletion testDocPath position expectedResults resetWorkspaceHook
        
            do! Completion.expectCompletion testDocPath position Completion.DefaultExpectedTerms resetWorkspaceHook
        }
    ]