module Ubictionary.VsCodeExtension.Tests.Completion

open Fable.Mocha
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Ubictionary.VsCodeExtension
open Node.Api

let tests =
    testList "Ubictionary Completion Tests" [

        testCaseAsync "Completion returns expected list" << Async.AwaitPromise <| promise {
            let docUri = vscode.Uri.file(path.resolve(__dirname,"../test/fixtures/simple_workspace/test.txt"));
            let! doc = workspace.openTextDocument(docUri)
            window.showTextDocument(doc, ViewColumn.Active, false) |> ignore
            let! _ = Helpers.getLanguageClient()
            do! Async.Sleep 100 |> Async.StartAsPromise
            let! result = commands.executeCommand(
                "vscode.executeCompletionItemProvider",
                Some (docUri :> obj),
                Some (vscode.Position.Create(0.0, 10.0) :> obj)
            )
            Expect.isSome result "Expect a result from the executeCompletionItemProvider"
            match result with
            | (Some completionResult: CompletionList option) ->
                let comparison =
                    completionResult.items
                    |> Seq.map (fun i ->
                        match i.label with
                        | U2.Case1 l -> l
                        | U2.Case2 ll -> ll.label)
                    |> Seq.compareWith compare
                    <| seq {"context"; "definitions"; "language"; "term"; "usage"}
                Expect.equal comparison 0 "Should have correct completion list"
            | None -> ()
        }
    ]