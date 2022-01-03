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

        let getConfig() = workspace.getConfiguration("contextly")

        let getFullPathFromConfig() =
            let config = getConfig()
            let path = config["path"].Value :?> string
            let wsf = workspace.workspaceFolders.Value[0]
            vscode.Uri.joinPath(wsf.uri, [|path|])

        let findUriInVisibleEditors (path:Uri) =
            window.visibleTextEditors.Find(fun te -> te.document.uri.path = path.path)

        testCaseAsync "Initialize Command should open existing definitions.yml" <| async {
            do! VsCodeCommands.initialize() |> Promise.Ignore

            let fullPath = getFullPathFromConfig()

            let editor = findUriInVisibleEditors fullPath

            do! closeDocument fullPath |> Promise.Ignore

            Expect.isNotNull editor "Existing definitions.yml isn't open"
        }

        let updateConfig newPath = promise {
            let config = getConfig()
            do! config.update("path", newPath :> obj |> Some)
            do! Promise.sleep 10
        }

        let resetConfig() = promise {
            let config = getConfig()
            do! config.update("path", None)
        }

        let resetDefinitionsFile fullPath = promise {
            do! closeDocument fullPath |> Promise.Ignore
            do! workspace.fs.delete fullPath
        }

        let resetWorkspace fullPath = promise {
            do! resetConfig()
            do! resetDefinitionsFile fullPath
        }

        testCaseAsync "Initialize Command should open new definitions.yml" <| async {
            let newPath = $"{System.Guid.NewGuid().ToString()}.yml"
            do! updateConfig newPath

            do! VsCodeCommands.initialize() |> Promise.Ignore
            
            let fullPath = getFullPathFromConfig()
            let editor = findUriInVisibleEditors fullPath

            do! resetWorkspace fullPath

            Expect.isNotNull editor "New definitions.yml isn't open"
        }
    ]
