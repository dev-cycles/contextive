module Contextly.VsCodeExtension.Tests.Extension

open Fable.Mocha
open Fable.Import.VSCode.Vscode

let tests =
    testList "Contextly Activation Tests" [
        testCase "Extension is Active" <| fun () ->
            printfn "Starting Extension is Active"
            let extension = extensions.all.Find(fun x -> x.id = "devcycles.contextly")
            Expect.equal extension.isActive true "Extension is not active"
            printfn "Ending Extension is Active"

        testCase "Extension has path config" <| fun () -> 
            printfn "Starting Extension has path config"
            let config = workspace.getConfiguration("contextly")
            let path = config["path"]
            Expect.isSome path "contextly.path config is not present"
            Expect.equal path.Value ".contextly/definitions.yml" "contextly.path config is not the default value"
            printfn "Ending Extension has path config"

        testCaseAsync "Language Client becomes Ready" <| async {
            printfn "Starting Language Client becomes Ready"
            do! Helpers.getLanguageClient() |> awaitP |> Async.Ignore
            printfn "Ending Language Client becomes Ready"
        }
    ]