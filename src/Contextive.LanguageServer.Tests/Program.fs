module Contextive.LanguageServer.Tests.Program

open Expecto
open Contextive.LanguageServer.Program

[<EntryPoint>]
let main argv =
    setupLogging
    runTestsInAssemblyWithCLIArgs [] argv