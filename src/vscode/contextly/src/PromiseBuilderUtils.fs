[<AutoOpen>]
module PromiseBuilderUtils

// From: https://github.com/fable-compiler/fable-promise/blob/c4efc8e46f48b415dc253987fe7a1bfb6836b800/docsrc/documentation/computation-expression.md
// Transform a thenable into a promise
open Fable.Core
open Fable.Import.VSCode

let private toPromise (t: Thenable<'T>): JS.Promise<'T> = unbox t

type Promise.PromiseBuilder with
    /// To make a value interop with the promise builder, you have to add an
    /// overload of the `Source` member to convert from your type to a promise.
    /// Because thenables are trivially convertible, we can just unbox them.
    member _.Source(t: Thenable<'T>): JS.Promise<'T> = toPromise t

    // Also provide these cases for overload resolution
    member _.Source(p: JS.Promise<'T>): JS.Promise<'T> = p
    member _.Source(ps: #seq<_>): _ = ps