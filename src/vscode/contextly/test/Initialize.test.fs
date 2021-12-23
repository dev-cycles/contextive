module Contextly.VsCodeExtension.Tests.Initialize

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextly.VsCodeExtension.Tests.Helpers

let tests =
    testList "Initialize Tests" [
        testCasePromise "Initialize Command should open existing definitions.yml" <| promise {
            do! getLanguageClient() |> Promise.Ignore

            let! _ = VsCodeCommands.initialize()

            let config = workspace.getConfiguration("contextly")
            let path = config["path"].Value :?> string
            let wsf = workspace.workspaceFolders.Value[0]
            let fullPath = vscode.Uri.joinPath(wsf.uri, [|path|])

            Expect.equal fullPath.path window.activeTextEditor.Value.document.uri.path "Existing definitions.yml isn't open"

            return! VsCodeCommands.closeActiveEditor() |> Promise.Ignore
        }
    ]
