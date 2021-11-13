module Ubictionary.VsCodeExtension.Tests.Extension

open Fable.Core
open Fable.Mocha
open Fable.Import.VSCode.Vscode

let tests =
    testList "Ubictionary Activation Tests" [
        testCase "Extension is Active" <| fun () ->
            let extension = extensions.all.Find(fun x -> x.id = "devcycles.ubictionary")
            Expect.equal extension.isActive true "Extension is not active"

        testCase "Extension has path config" <| fun () -> 
            let config = workspace.getConfiguration("ubictionary", ConfigurationScope.Case5 "")
            let path = config.get("path")
            Expect.isSome path "ubictionary.path config is not present"
            Expect.equal path.Value ".ubictionary/definitions.yml" "ubictionary.path config is not the default value"

        testCaseAsync "Language Client becomes Ready" <| async {
            do! Helpers.getLanguageClient() |> Async.AwaitPromise |> Async.Ignore
        }
    ]