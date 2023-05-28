module Contextive.VsCodeExtension.Tests.MultiRoot.Completion

open Fable.Mocha
open Fable.Import.VSCode
open Contextive.VsCodeExtension.TestHelpers
open Contextive.VsCodeExtension.TestHelpers.Helpers
open Contextive.VsCodeExtension.TestHelpers.Completion

let tests =

    let completionTest ((root:string),(expectedTerms: string seq)) = 
        testCaseAsync $"finds terms for {root}" <| async {
            let testDocPath = Paths.inWorkspace $"{root}/empty.txt"
            let position = vscode.Position.Create(0, 10)
            let expectedTerms = expectedTerms
            do! expectCompletion testDocPath position expectedTerms None
        }

    [
        ("rootA", seq {"A"; "both"})
        ("rootB", seq {"B"; "both"})
    ] |> List.map completionTest |> testList "Completion in a root"