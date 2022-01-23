module Contextive.VsCodeExtension.Thenable

open Fable.Core
open Fable.Import.VSCode

let Ignore (t: Thenable<'t>): JS.Promise<unit> = promise {
    let! _ = t
    ()
}