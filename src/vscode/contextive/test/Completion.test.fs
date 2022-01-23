module Contextive.VsCodeExtension.Tests.Completion

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension.Tests.Helpers

let getLabels (items:ResizeArray<CompletionItem>) =
    items
    |> Seq.map (fun item ->
        match item.label with
        | U2.Case2 ll -> ll.label
        | U2.Case1 l -> l)

let expectCompletion path position expectedResults (onBeforeAssert : (unit -> JS.Promise<unit>) option)= promise {
    let! docUri = getDocUri path |> openDocument

    let! result = VsCodeCommands.complete docUri <| position

    do! getDocUri path |> closeDocument

    match onBeforeAssert with
    | Some oba -> do! oba()
    | None -> ()

    match result with
    | (Some completionResult: CompletionList option) ->
        let labels = getLabels completionResult.items
        Expect.seqEqual expectedResults labels "executeCompletionProvider should return expected completion items"
    | None ->
        Expect.isSome result "executeCompletionItemProvider should return a result"
}

let DefaultExpectedTerms = seq {"context"; "definition"; "example"; "term"}

let tests =
    testList "Contextive Completion Tests" [

        testCaseAsync "Completion returns expected list" <| async {
            let testDocPath = "../test/fixtures/simple_workspace/test.txt"
            let position = vscode.Position.Create(0.0, 10.0)
            do! expectCompletion testDocPath position DefaultExpectedTerms None
        }
    ]