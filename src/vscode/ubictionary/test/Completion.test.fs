module Ubictionary.VsCodeExtension.Tests.Completion

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Ubictionary.VsCodeExtension
open Node.Api

let tests =
    testList "Ubictionary Completion Tests" [

        testCaseAsync "Completion returns expected list" <| async {
            let docUri = vscode.Uri.file(path.resolve(__dirname,"../test/fixtures/simple_workspace/test.txt"));
            let! doc = workspace.openTextDocument(docUri) |> Async.AwaitThenable
            window.showTextDocument(doc, ViewColumn.Active, false) |> ignore
            let! _ = Helpers.getLanguageClient()
            do! Async.Sleep 500
            let! result = commands.executeCommand(
                                "vscode.executeCompletionItemProvider",
                                Some (docUri :> obj),
                                Some (vscode.Position.Create(0.0, 10.0) :> obj)
                            ) |> Async.AwaitThenable
            Expect.isSome result "Expect a result from the executeCompletionItemProvider"
            match result with
            | (Some completionResult: CompletionList option) ->
                let labels =
                    completionResult.items
                    |> Seq.map (fun i ->
                        match i.label with
                        | U2.Case1 l -> l
                        | U2.Case2 ll -> ll.label)
                let expected = seq {"context"; "definitions"; "language"; "term"; "usage"}
                let comparison = Seq.compareWith compare expected labels
                Expect.equal comparison 0 (sprintf "Should have correct completion list.\nExpected:\n- %A;\nActual:\n- %A" (expected |> String.concat "\n- ") (labels |> String.concat "\n- "))
            | None -> ()
        }
    ]