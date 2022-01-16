module Contextly.VsCodeExtension.Tests.Main

open Fable.Mocha
open Fable.Core
open Fable.Core.JS

// Import mocha explicitly.  Fable.Mocha assumes running via the mocha CLI which imports mocha _implicitly_
[<Import("*", from="mocha")>]
let Mocha: obj = jsNative

[<Emit("afterEach($0)")>]
let afterEach (callback: unit -> Promise<unit>):unit = jsNative

afterEach(fun () -> promise {
    do! Initialize.resetConfig()
})

Fable.Mocha.Mocha.runTests <| testList "All" [
    Extension.tests
    Completion.tests
    Initialize.tests
    InvalidSchema.tests
] |> ignore

