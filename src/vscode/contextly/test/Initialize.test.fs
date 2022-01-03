module Contextly.VsCodeExtension.Tests.Initialize

open Fable.Mocha
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextly.VsCodeExtension.Tests.Helpers

let tests =
    testList "Initialize Tests" [
        testCaseAsync "Extension has Initialize Project Command" <| async {
            let! registeredCommands = commands.getCommands(false)
            Expect.exists registeredCommands (fun c -> c = "contextly.initialize") "Initialize command doesn't exist"
        }

        testCaseAsync "Initialize Command should open existing definitions.yml" <| async {
            do! waitForLanguageClient() |> Promise.Ignore

            do! VsCodeCommands.initialize() |> Promise.Ignore

            let config = workspace.getConfiguration("contextly")
            let path = config["path"].Value :?> string
            let wsf = workspace.workspaceFolders.Value[0]
            let fullPath = vscode.Uri.joinPath(wsf.uri, [|path|])

            let editor = window.visibleTextEditors.Find(fun te -> te.document.uri.path = fullPath.path)

            do! closeDocument fullPath |> Promise.Ignore

            Expect.isNotNull editor "Existing definitions.yml isn't open"
        }

        testCaseAsync "Initialize Command should open new definitions.yml" <| async {
            do! waitForLanguageClient() |> Promise.Ignore

            let config = workspace.getConfiguration("contextly")
            let newPath = $"{System.Guid.NewGuid().ToString()}.yml"
            do! config.update("path", newPath :> obj |> Some)
            do! Promise.sleep 10
            
            do! VsCodeCommands.initialize() |> Promise.Ignore
            let activeEditor = window.activeTextEditor.Value

            do! config.update("path", None)

            let wsf = workspace.workspaceFolders.Value[0]
            let fullPath = vscode.Uri.joinPath(wsf.uri, [|newPath|])
            do! closeDocument fullPath |> Promise.Ignore
            do! workspace.fs.delete fullPath

            Expect.equal fullPath.path activeEditor.document.uri.path "New definitions.yml isn't open"
        }
    ]
