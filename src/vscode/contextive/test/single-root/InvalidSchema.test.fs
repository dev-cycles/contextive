module Contextive.VsCodeExtension.Tests.E2E.SingleRoot.InvalidSchema

open Fable.Mocha
open Fable.Import.VSCode
open Contextive.VsCodeExtension.Tests.E2E.Helpers
open Contextive.VsCodeExtension.Tests.E2E.Helpers.Helpers

let tests =
    testList
        "Invalid Schema Tests"
        [ testCaseAsync "Should recover from an invalid schema"
          <| async {
              do! updateConfig ".contextive/invalid_schema.yml"

              let testDocPath = Paths.inWorkspace "test.txt"
              let position = vscode.Position.Create(0, 10)

              do!
                  Completion.expectCompletion testDocPath position
                  <| seq {
                      "some"
                      "text"
                  }
                  <| None

              do! resetConfig ()

              do! Completion.expectCompletion testDocPath position Completion.DefaultExpectedTerms None
          } ]
