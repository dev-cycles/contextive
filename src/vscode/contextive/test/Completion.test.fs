module Contextive.VsCodeExtension.Tests.Completion

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension.Tests.Helpers

let private getCompletionLabelsFromItems (items:ResizeArray<CompletionItem>) =
    items
    |> Seq.map (fun item ->
        match item.label with
        | U2.Case2 ll -> ll.label
        | U2.Case1 l -> l)

let private getCompletionLabels (result:CompletionList option) =
    match result with
    | (Some completionResult: CompletionList option) ->
        getCompletionLabelsFromItems completionResult.items
    | None ->
        seq []

let rec expectCompletion path position expectedResults (onBeforeAssert : (unit -> JS.Promise<unit>) option) = promise {
    let! docUri = getDocUri path |> openDocument

    do! Promise.sleep(250)

    let! result = VsCodeCommands.complete docUri position

    do! getDocUri path |> closeDocument

    let labels = getCompletionLabels result

    match onBeforeAssert with
    | Some oba -> do! oba()
    | None -> ()

    Expect.seqEqual expectedResults labels "executeCompletionProvider should return expected completion items"
}

let DefaultExpectedTerms = seq {"context"; "definition"; "example"; "some"; "term"; "text"}

let tests =
    testList "Contextive Completion Tests" [

        testCaseAsync "Completion returns expected list" <| async {
            let testDocPath = "../test/fixtures/simple_workspace/test.txt"
            let position = vscode.Position.Create(0, 10)
            do! expectCompletion testDocPath position DefaultExpectedTerms None
        }
    ]