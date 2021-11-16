module Promise

open Fable.Core

let Ignore (p: JS.Promise<'t>): JS.Promise<unit> = promise {
    let! _ = p
    ()
}