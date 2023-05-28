module Contextive.VsCodeExtension.Tests.MultiRoot.Main

open Fable.Mocha
open Fable.Core
open Contextive.VsCodeExtension.TestHelpers
open Contextive.VsCodeExtension.TestHelpers.Helpers

// Import mocha explicitly.  Fable.Mocha assumes running via the mocha CLI which imports mocha _implicitly_
[<Import("*", from="mocha")>]
let Mocha: obj = jsNative

Fable.Mocha.Mocha.runTests <| testList "Multi-Root Workspace" [
    Completion.tests
] |> ignore