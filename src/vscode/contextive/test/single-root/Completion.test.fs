module Contextive.VsCodeExtension.Tests.SingleRoot.Completion

open Fable.Mocha
open Fable.Import.VSCode
open Contextive.VsCodeExtension.TestHelpers
open Contextive.VsCodeExtension.TestHelpers.Completion

let DefaultExpectedTerms = seq {"context"; "definition"; "example"; "some"; "term"; "text"}

let tests =
    testList "Contextive Completion Tests" [

        testCaseAsync "Completion returns expected list" <| async {
            let testDocPath = Paths.inWorkspace "test.txt"
            let position = vscode.Position.Create(0, 10)
            do! expectCompletion testDocPath position DefaultExpectedTerms None
        }
    ]