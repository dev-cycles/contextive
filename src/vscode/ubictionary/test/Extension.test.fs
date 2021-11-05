module Ubictionary.VsCodeExtension.Tests.Extension

open Fable.Mocha
open Ubictionary.VsCodeExtension.Extension

let tests =
    testList "Extension Test Suite" [
        testCase "Sample test" <| fun () ->
            Expect.equal 5 5 "Numbers equal"

        testCase "Dependency Test" <| fun () ->
            Expect.equal (testFn 5) 10 "Func Result equal"
    ]