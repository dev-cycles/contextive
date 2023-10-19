module Contextive.LanguageServer.Configuration

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol.Models

let resolvedPathGetter configGetter pathResolver () =
    async {
        let! path = configGetter ()
        return pathResolver path
    }

type Config = { mutable Path: string option }

let handler (config: Config) (definitionsLoader: Definitions.Reloader) (configParams: DidChangeConfigurationParams) =
    let path = configParams.Settings.Item("contextive").Item("path")
    config.Path <- Some(path.ToString())
    definitionsLoader ()
    Task.CompletedTask
