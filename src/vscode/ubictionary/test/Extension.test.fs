module Ubictionary.VsCodeExtension.Tests.Extension

open Fable.Mocha
open Fable.Core
open Ubictionary.VsCodeExtension.Extension

// Import mocha explicitly.  Fable.Mocha assumes running via the mocha CLI which imports mocha _implicitly_
[<Import("*", from="mocha")>]
let Mocha: obj = jsNative

let extensionTests =
    testList "Extension Test Suite" [
        testCase "Sample test" <| fun () ->
            Expect.equal 5 5 "Numbers equal"

        testCase "Dependency Test" <| fun () ->
            Expect.equal (testFn 5) 10 "Func Result equal"
    ]

Fable.Mocha.Mocha.runTests extensionTests |> ignore