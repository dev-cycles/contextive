module Contextive.VsCodeExtension.Tests.E2E.SingleRoot.Extension

open Fable.Mocha
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension.Extension

let tests =
    testList
        "Contextive Activation Tests"
        [ testCase "Contextive is Active"
          <| fun () ->
              let extension = extensions.all.Find(fun x -> x.id = "devcycles.contextive")
              Expect.equal extension.isActive true "Extension is not active"

          testCase "CSharp Extension is Active"
          <| fun () ->
              let extension = extensions.all.Find(fun x -> x.id = "ms-dotnettools.csharp")
              Expect.equal extension.isActive true "Extension is not active"

          testCase "Extension has no default path config"
          <| fun () ->
              let config = workspace.getConfiguration ("contextive")
              let path = config["path"]
              Expect.isNone path "contextive.path config is present"

          //   Expect.equal
          //       path.Value
          //       ".contextive/definitions.yml"
          //       "contextive.path config is not the default value - it must match the default configured in the language server"

          testCaseAsync "Language Client becomes Ready"
          <| async { getLanguageClient () |> ignore } ]
