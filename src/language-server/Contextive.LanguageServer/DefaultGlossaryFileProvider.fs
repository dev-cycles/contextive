module Contextive.LanguageServer.DefaultGlossaryFileProvider

open Contextive.Core.File
open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Window
open Serilog

let ConfigSection = "contextive"
let private pathKey = "path"

[<Literal>]
let private defaultContextiveGlossaryFilePath = ".contextive/definitions.yml"

let private getConfigValue (s: ILanguageServer) section key =
    async {
        Log.Logger.Information $"Getting {section} {key} config..."

        let! config =
            // We need to use `GetScopedConfiguration` because of https://github.com/OmniSharp/csharp-language-server-protocol/issues/1101
            // We can revert to `GetConfiguration` when that bug is fixed.
            s.Configuration.GetScopedConfiguration("file:///", System.Threading.CancellationToken.None)
            //s.Configuration.GetConfiguration(ConfigurationItem(Section = configSection))
            |> Async.AwaitTask

        let configValue = config.GetSection(section).Item(key)

        let value =
            if System.String.IsNullOrEmpty configValue then
                match key with
                | key when key = pathKey ->
                    Some
                        { Path = defaultContextiveGlossaryFilePath
                          IsDefault = true }
                | _ -> None
            else
                Some
                    { Path = configValue
                      IsDefault = false }

        Log.Logger.Information $"Got {key} {value}"
        return value
    }

let initGlossaryFileInitializer (s: ILanguageServer) pathGetter =
    let showDocument =
        match s.ClientSettings.Capabilities.Window.ShowDocument.IsSupported with
        | true -> s.Window.ShowDocument
        | false -> fun _ -> Task.FromResult(ShowDocumentResult())

    GlossaryFileInitializer.registerHandler s pathGetter showDocument

let private getResolvedPathGetter (s: ILanguageServer) =
    async {
        let configGetter () = getConfigValue s ConfigSection pathKey
        // Not sure if this is needed to ensure configuration is loaded, or allow a task/context switch
        // Either way, if it's not here, then getWorkspaceFolder returns null
        let! _ = configGetter ()

        let workspaceFolder = Configuration.getWorkspaceFolder s

        return
            PathResolver.resolvePath workspaceFolder
            |> Configuration.resolvedPathGetter configGetter
    }

let getDefaultGlossaryFilePathResolver (s: ILanguageServer) =
    async {
        let! pathGetter = getResolvedPathGetter s

        initGlossaryFileInitializer s pathGetter

        return
            fun () ->
                async {
                    let! path = pathGetter ()

                    return path |> Result.mapError FileError.PathInvalid
                }

    }
