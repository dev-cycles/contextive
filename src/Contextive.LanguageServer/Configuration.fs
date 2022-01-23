module Contextive.LanguageServer.Configuration

open System.Threading.Tasks

let handler (definitionsLoader:Definitions.Reloader) _ =
    definitionsLoader()
    Task.CompletedTask