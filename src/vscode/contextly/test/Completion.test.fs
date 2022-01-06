module Contextly.VsCodeExtension.Tests.Completion

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextly.VsCodeExtension.Tests.Helpers

let private getLabels (items:ResizeArray<CompletionItem>) =
    items
    |> Seq.map (fun item ->
        match item.label with
        | U2.Case2 ll -> ll.label
        | U2.Case1 l -> l)

let tests =
    testList "Contextly Completion Tests" [

        testCaseAsync "Completion returns expected list" <| async {
            let testDocPath = "../test/fixtures/simple_workspace/test.txt"
            let! docUri = getDocUri testDocPath |> openDocument

            let! result = VsCodeCommands.complete docUri <| vscode.Position.Create(0.0, 10.0)

            do! getDocUri testDocPath |> closeDocument

            match result with
            | (Some completionResult: CompletionList option) ->
                let labels = getLabels completionResult.items
                let expected = seq {"context"; "definition"; "example"; "term"}
                Expect.seqEqual expected labels "executeCompletionProvider should return expected completion items"
            | None ->
                Expect.isSome result "executeCompletionItemProvider should return a result"
        }
    ]