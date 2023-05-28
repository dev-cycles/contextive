module Contextive.VsCodeExtension.Tests.SingleRoot.Extension

open Fable.Mocha
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension.Extension

let tests =
    testList "Contextive Activation Tests" [
        testCase "Contextive is Active" <| fun () ->
            let extension = extensions.all.Find(fun x -> x.id = "devcycles.contextive")
            Expect.equal extension.isActive true "Extension is not active"

        testCase "CSharp Extension is Active" <| fun () ->
            let extension = extensions.all.Find(fun x -> x.id = "ms-dotnettools.csharp")
            Expect.equal extension.isActive true "Extension is not active"

        testCase "Extension has path config" <| fun () -> 
            let config = workspace.getConfiguration("contextive")
            let path = config["path"]
            Expect.isSome path "contextive.path config is not present"
            Expect.equal path.Value ".contextive/definitions.yml" "contextive.path config is not the default value"

        testCaseAsync "Language Client becomes Ready" <| async {
            getLanguageClient() |> ignore
        }
    ]