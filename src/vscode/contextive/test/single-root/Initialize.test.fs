module Contextive.VsCodeExtension.Tests.E2E.SingleRoot.Initialize

open Fable.Mocha
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension.Tests.E2E.Helpers
open Contextive.VsCodeExtension.Tests.E2E.Helpers.Helpers

let tests =
    testList
        "Initialize Tests"
        [ testCaseAsync "Extension has Initialize Project Command"
          <| async {
              let! registeredCommands = commands.getCommands (false)
              Expect.exists registeredCommands (fun c -> c = "contextive.initialize") "Initialize command doesn't exist"
          }

          testCaseAsync
              "When config is undefined, and default definitions file doesn't exist, initialize Command should create and open in default location"
          <| async {

              do! resetConfig ()

              let fullPath = getDefaultPath ()

              do! deleteFile fullPath

              do! VsCodeCommands.initialize () |> Promise.Ignore

              let editor = findUriInVisibleEditors fullPath

              do! closeDocument fullPath |> Promise.Ignore

              Expect.isNotNull editor "Existing definitions.yml isn't open"
          }

          testCaseAsync "When definitions file exists, Initialize Command should open existing file"
          <| async {
              do! VsCodeCommands.initialize () |> Promise.Ignore

              let fullPath = getFullPathFromConfig ()

              let editor = findUriInVisibleEditors fullPath

              do! closeDocument fullPath |> Promise.Ignore

              Expect.isNotNull editor "Existing definitions.yml isn't open"
          }

          testCaseAsync
              "When config is defined, and definitions file doesn't exist, Initialize Command should create and open in configured location"
          <| async {
              let newPath = $"{System.Guid.NewGuid().ToString()}.yml"
              do! updateConfig newPath

              do! VsCodeCommands.initialize () |> Promise.Ignore

              let fullPath = getFullPathFromConfig ()
              let editor = findUriInVisibleEditors fullPath

              do! deleteConfiguredDefinitionsFile ()

              Expect.isNotNull editor "New definitions.yml isn't open"
          }

          let getContent (content: string) =
              System.Text.Encoding.UTF8.GetBytes(content)
              |> Fable.Core.JS.Constructors.Uint8Array.Create

          testCaseAsync "New definitions file should show new term in completion"
          <| async {
              let newPath = $"{System.Guid.NewGuid().ToString()}.yml"
              do! updateConfig newPath

              do! VsCodeCommands.initialize () |> Promise.Ignore

              let fullPath = getFullPathFromConfig ()

              let content =
                  getContent
                      """contexts:
  - terms:
    - name: anewterm"""

              do! workspace.fs.writeFile (fullPath, content)

              do! Promise.sleep (500)

              let resetWorkspaceHook = Some deleteConfiguredDefinitionsFile

              let testDocPath = Paths.inWorkspace "test.txt"
              let position = vscode.Position.Create(0, 10)

              let expectedResults =
                  seq {
                      "anewterm"
                      "some"
                      "text"
                  }

              do! Completion.expectCompletion testDocPath position expectedResults resetWorkspaceHook
          } ]
