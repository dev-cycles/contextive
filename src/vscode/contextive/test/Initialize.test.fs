module Contextive.VsCodeExtension.Tests.Initialize

open Fable.Mocha
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension.Tests.Helpers

let tests =
    testList "Initialize Tests" [
        testCaseAsync "Extension has Initialize Project Command" <| async {
            let! registeredCommands = commands.getCommands(false)
            Expect.exists registeredCommands (fun c -> c = "contextive.initialize") "Initialize command doesn't exist"
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

            do! deleteDefinitionsFile()

            Expect.isNotNull editor "New definitions.yml isn't open"
        }

        let getContent (content:string) = System.Text.Encoding.UTF8.GetBytes(content) |> Fable.Core.JS.Constructors.Uint8Array.Create

        testCaseAsync "New definitions file should show new term in completion" <| async {
            let newPath = $"{System.Guid.NewGuid().ToString()}.yml"
            do! updateConfig newPath

            do! VsCodeCommands.initialize() |> Promise.Ignore
            
            let fullPath = getFullPathFromConfig()

            let content = getContent """contexts:
  - terms:
    - name: anewterm"""

            do! workspace.fs.writeFile(fullPath, content)

            do! Promise.sleep(500)

            let resetWorkspaceHook = Some deleteDefinitionsFile

            let testDocPath = "../test/fixtures/simple_workspace/test.txt"
            let position = vscode.Position.Create(0, 10)
            let expectedResults = seq {"anewterm"; "some"; "text"}
            do! Completion.expectCompletion testDocPath position expectedResults resetWorkspaceHook
        }
    ]
