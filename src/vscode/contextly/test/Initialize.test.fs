module Contextly.VsCodeExtension.Tests.Initialize

open Fable.Mocha
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextly.VsCodeExtension.Tests.Helpers

let tests =
    testList "Initialize Tests" [
        testCaseAsync "Extension has Initialize Project Command" <| async {
            printfn "Starting Extension has Initialize Project Command"
            let! registeredCommands = awaitT (commands.getCommands(false))
            Expect.exists registeredCommands (fun c -> c = "contextly.initialize") "Initialize command doesn't exist"
            printfn "Ending Extension has Initialize Project Command"
        }

        testCaseAsync "Initialize Command should open existing definitions.yml" <| async {
            printfn "Starting Initialize Command should open existing definitions.yml"
            do! getLanguageClient() |> awaitP |> Async.Ignore

            let! _ = awaitP (VsCodeCommands.initialize())

            let config = workspace.getConfiguration("contextly")
            let path = config["path"].Value :?> string
            let wsf = workspace.workspaceFolders.Value[0]
            let fullPath = vscode.Uri.joinPath(wsf.uri, [|path|])

            let editor = window.visibleTextEditors.Find(fun te -> te.document.uri.path = fullPath.path)

            do! closeDocument fullPath |> awaitP |> Async.Ignore

            Expect.isNotNull editor "Existing definitions.yml isn't open"
            printfn "Ending Initialize Command should open existing definitions.yml"
        }

        // testCasePromise "Initialize Command should open new definitions.yml" <| promise {
        //     do! getLanguageClient() |> Promise.Ignore

        //     do! VsCodeCommands.closeAllEditors() |> Promise.Ignore

        //     let config = workspace.getConfiguration("contextly")
        //     let existingPath = config["path"].Value :?> string
        //     printfn "Existing path: %A" existingPath
        //     let newPath = $"{System.Guid.NewGuid().ToString()}.yml"
        //     let! _ = config.update("path", newPath :> obj |> Some)
        //     do! Promise.sleep 1000
        //     let config = workspace.getConfiguration("contextly")
        //     let newPathFromConfig = config["path"].Value :?> string
        //     printfn "New PathFrom Config: %A" newPathFromConfig
        //     let! _ = VsCodeCommands.initialize()

        //     let wsf = workspace.workspaceFolders.Value[0]
        //     let fullPath = vscode.Uri.joinPath(wsf.uri, [|newPath|])

        //     let! _ = config.update("path", None)
        //     let activeEditor = window.activeTextEditor.Value
        //     do! VsCodeCommands.closeActiveEditor() |> Promise.Ignore

        //     Expect.equal fullPath.path activeEditor.document.uri.path "New definitions.yml isn't open"
        // }
    ]
