module Ubictionary.VsCodeExtension.Tests.Extension

open Fable.Mocha
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.VSCode.Vscode
open Fable.Import.LanguageServer
open Ubictionary.VsCodeExtension.Extension

// Import mocha explicitly.  Fable.Mocha assumes running via the mocha CLI which imports mocha _implicitly_
[<Import("*", from="mocha")>]
let Mocha: obj = jsNative

let extensionTests =
    testList "Ubictionary Activation Tests" [
        testCase "Extension is Active" <| fun () ->
            let extension = extensions.all.Find(fun x -> x.id = "devcycles.ubictionary")
            Expect.equal extension.isActive true "Extension is not active"

        testCase "Extension has path config" <| fun () -> 
            let config = workspace.getConfiguration("ubictionary", ConfigurationScope.Case5 "")
            let path = config.get("path")
            Expect.isSome path "ubictionary.path config is not present"
            Expect.equal path.Value "./ubictionary/definitions.yml" "ubictionary.path config is not the default value"

        testCaseAsync "Language Client becomes Ready" <| async {
            let extension = extensions.all.Find(fun x -> x.id = "devcycles.ubictionary")
            let extensionApi: Api = !!extension.exports.Value
            let languageClient:LanguageClient = extensionApi.Client
            do! languageClient.onReady() |> Async.AwaitPromise
        }
    ]

Fable.Mocha.Mocha.runTests extensionTests |> ignore