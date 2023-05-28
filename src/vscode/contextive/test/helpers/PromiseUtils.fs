[<AutoOpen>]
module Contextive.VsCodeExtension.TestHelpers.PromiseUtils

open Fable.Core
open Fable.Import.VSCode

let private toPromise (t: Thenable<'T>): JS.Promise<'T> = unbox t

type AsyncBuilder with
    member _.Source(t: Thenable<'T>): Async<'T> = toPromise t |> Async.AwaitPromise
    member _.Source(p: JS.Promise<'T>): Async<'T> = p |> Async.AwaitPromise