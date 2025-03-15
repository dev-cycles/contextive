module Contextive.VsCodeExtension.Tests.E2E.SingleRoot.Initialize

open Fable.Mocha
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension
open Contextive.VsCodeExtension.Tests.E2E.Helpers
open Contextive.VsCodeExtension.Tests.E2E.Helpers.Helpers

let tests =
    testList
        "Initialize Tests"
        [ testCaseAsync "Extension has Initialize Project Command"
          <| async {
              let! registeredCommands = commands.getCommands false
              Expect.exists registeredCommands (fun c -> c = "contextive.initialize") "Initialize command doesn't exist"
          }

          testCaseAsync
              "When config is undefined, and default glossary file doesn't exist, initialize Command should create and open in default location"
          <| async {

              do! resetConfig ()

              let fullPath = getDefaultPath ()

              do! deleteFile fullPath

              do! VsCodeCommands.initialize () |> Promise.Ignore

              do! Waiter.waitFor <| documentIsOpen fullPath

              let editor = findUriInVisibleEditors fullPath

              do! closeDocument fullPath |> Promise.Ignore

              Expect.isNotNull editor "Existing definitions.yml isn't open"
          }

          testCaseAsync "When glossary file exists, Initialize Command should open existing file"
          <| async {
              do! VsCodeCommands.initialize () |> Promise.Ignore

              let fullPath = getDefaultPath ()

              do! Waiter.waitFor <| documentIsOpen fullPath

              let editor = findUriInVisibleEditors fullPath

              do! closeDocument fullPath |> Promise.Ignore

              Expect.isNotNull editor "Existing definitions.yml isn't open"
          }

          testCaseAsync
              "When config is defined, and glossary file doesn't exist, Initialize Command should create and open in configured location"
          <| async {
              let newPath = $".contextive/{System.Guid.NewGuid().ToString()}.yml"
              do! updateConfig newPath

              do! VsCodeCommands.initialize () |> Promise.Ignore

              let fullPath = getFullPathFromConfig ()

              do! Waiter.waitFor <| documentIsOpen fullPath

              let editor = findUriInVisibleEditors fullPath

              do! deleteConfiguredGlossaryFile ()

              Expect.isNotNull editor "New definitions.yml isn't open"
          }

          testCaseAsync "New glossary file should show new term in completion"
          <| async {
              let newPath = $".contextive/{System.Guid.NewGuid().ToString()}.yml"
              do! updateConfig newPath

              do! VsCodeCommands.initialize () |> Promise.Ignore

              let fullPath = getFullPathFromConfig ()

              let content =
                  """contexts:
  - terms:
    - name: anewterm"""

              do! writeFile fullPath content

              let resetWorkspaceHook = Some deleteConfiguredGlossaryFile

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
