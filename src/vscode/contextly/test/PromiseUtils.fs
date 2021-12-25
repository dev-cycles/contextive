[<AutoOpen>]
module Contextly.VsCodeExtension.Tests.Extensions

open Fable.Core
open Fable.Core.JS
open Fable.Mocha

let testCasePromise (name:string) (testPromise:Promise<unit>) = 
    testCaseAsync name <| async {
        let! _ = Async.AwaitPromise testPromise
        return ()
    }