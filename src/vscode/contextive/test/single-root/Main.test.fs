module Contextive.VsCodeExtension.Tests.E2E.SingleRoot.Main

open Fable.Mocha
open Fable.Core
open Contextive.VsCodeExtension.Tests.E2E.Helpers.Helpers

// Import mocha explicitly.  Fable.Mocha assumes running via the mocha CLI which imports mocha _implicitly_
[<Import("*", from = "mocha")>]
let Mocha: obj = jsNative

afterEach ("reset config", (fun () -> promise { do! resetConfig () }))

Fable.Mocha.Mocha.runTests
<| testList
    "Single-Root Workspace"
    [ Extension.tests
      Initialize.tests
      Completion.tests
      InvalidSchema.tests
      Hover.tests ]
|> ignore
