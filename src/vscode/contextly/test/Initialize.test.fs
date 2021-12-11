module Contextly.VsCodeExtension.Tests.Initialize

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextly.VsCodeExtension.Tests.Helpers

let tests =
    testList "Initialize Tests" [
        testCase "Initialize Command should open default definitions.yml" <| fun () ->
            ()
    ]
