module Ubictionary.VsCodeExtension.Tests.Completion

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Ubictionary.VsCodeExtension.Tests.Helpers

let private getLabels (items:ResizeArray<CompletionItem>) =
    items
    |> Seq.map (fun i ->
        match i.label with
        | U2.Case1 l -> l
        | U2.Case2 ll -> ll.label)

let tests =
    testList "Ubictionary Completion Tests" [

        testCasePromise "Completion returns expected list" <| promise {
            let! docUri = openDocument "../test/fixtures/simple_workspace/test.txt"

            do! getLanguageClient() |> Promise.Ignore

            let! result = VsCodeCommands.completion docUri <| vscode.Position.Create(0.0, 10.0)
            
            match result with
            | (Some completionResult: CompletionList option) ->
                let labels = getLabels completionResult.items
                let expected = seq {"context"; "definitions"; "language"; "term"; "usage"}
                Expect.seqEqual expected labels "executeCompletionProvider should return expected completion items"
            | None ->
                Expect.isSome result "executeCompletionItemProvider should return a result"
        }
    ]