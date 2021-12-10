[<AutoOpen>]
module Contextly.VsCodeExtension.Tests.Extensions

open Fable.Core
open Fable.Core.JS
open Fable.Mocha

let testCasePromise (name:string) (body:Promise<unit>) = AsyncTest(name, body |> Async.AwaitPromise, Normal)