module Ubictionary.VsCodeExtension.Tests.Main

open Fable.Mocha
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.VSCode.Vscode
open Fable.Import.LanguageServer
open Ubictionary.VsCodeExtension.Extension

// Import mocha explicitly.  Fable.Mocha assumes running via the mocha CLI which imports mocha _implicitly_
[<Import("*", from="mocha")>]
let Mocha: obj = jsNative

Fable.Mocha.Mocha.runTests <| testList "All" [
    Extension.tests
    Completion.tests
] |> ignore

