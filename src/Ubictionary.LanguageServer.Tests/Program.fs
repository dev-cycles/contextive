module Ubictionary.LanguageServer.Tests.Program

open Expecto
open Ubictionary.LanguageServer.Program

[<EntryPoint>]
let main argv =
    setupLogging
    runTestsInAssemblyWithCLIArgs [] argv