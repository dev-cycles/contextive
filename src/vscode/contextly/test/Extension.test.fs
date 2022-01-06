module Contextly.VsCodeExtension.Tests.Extension

open Fable.Mocha
open Fable.Import.VSCode.Vscode
open Contextly.VsCodeExtension.Extension

let tests =
    testList "Contextly Activation Tests" [
        testCase "Extension is Active" <| fun () ->
            let extension = extensions.all.Find(fun x -> x.id = "devcycles.contextly")
            Expect.equal extension.isActive true "Extension is not active"

        testCase "Extension has path config" <| fun () -> 
            let config = workspace.getConfiguration("contextly")
            let path = config["path"]
            Expect.isSome path "contextly.path config is not present"
            Expect.equal path.Value ".contextly/definitions.yml" "contextly.path config is not the default value"

        testCase "Language Client becomes Ready" <| fun () ->
            getLanguageClient() |> ignore
    ]