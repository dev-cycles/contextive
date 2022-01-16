module Contextly.VsCodeExtension.Tests.Initialize

open Fable.Mocha
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextly.VsCodeExtension.Tests.Helpers

let getConfig() = workspace.getConfiguration("contextly")

let getFullPathFromConfig() =
    let config = getConfig()
    let path = config["path"].Value :?> string
    let wsf = workspace.workspaceFolders.Value[0]
    vscode.Uri.joinPath(wsf.uri, [|path|])

let findUriInVisibleEditors (path:Uri) =
    window.visibleTextEditors.Find(fun te -> te.document.uri.path = path.path)

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

let tests =
    testList "Initialize Tests" [
        testCaseAsync "Extension has Initialize Project Command" <| async {
            let! registeredCommands = commands.getCommands(false)
            Expect.exists registeredCommands (fun c -> c = "contextly.initialize") "Initialize command doesn't exist"
        }

        testCaseAsync "Initialize Command should open existing definitions.yml" <| async {
            do! VsCodeCommands.initialize() |> Promise.Ignore

            let fullPath = getFullPathFromConfig()

            let editor = findUriInVisibleEditors fullPath

            do! closeDocument fullPath |> Promise.Ignore

            Expect.isNotNull editor "Existing definitions.yml isn't open"
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

        let getContent (content:string) = System.Text.Encoding.UTF8.GetBytes(content) |> Fable.Core.JS.Constructors.Uint8Array.Create

        testCaseAsync "New definitions file should work" <| async {
            let newPath = $"{System.Guid.NewGuid().ToString()}.yml"
            do! updateConfig newPath

            do! VsCodeCommands.initialize() |> Promise.Ignore
            
            let fullPath = getFullPathFromConfig()

            let content = getContent """contexts:
  - terms:
    - name: anewterm"""

            do! workspace.fs.writeFile(fullPath, content)

            do! Promise.sleep 500

            let resetWorkspaceHook = Some (fun _ -> promise {do! resetWorkspace fullPath})

            let testDocPath = "../test/fixtures/simple_workspace/test.txt"
            let position = vscode.Position.Create(0.0, 10.0)
            let expectedResults = seq {"anewterm"}
            do! Completion.expectCompletion testDocPath position expectedResults resetWorkspaceHook
        }
    ]
