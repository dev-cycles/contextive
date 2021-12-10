module Contextly.LanguageServer.Tests.Program

open Expecto
open Contextly.LanguageServer.Program

[<EntryPoint>]
let main argv =
    setupLogging
    runTestsInAssemblyWithCLIArgs [] argv