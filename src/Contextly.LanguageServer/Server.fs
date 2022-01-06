module Contextly.LanguageServer.Server

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Window
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open Microsoft.Extensions.Logging
open Serilog
open System.IO

let configSection = "contextly"
let pathKey = "path"

let private getConfig (s:ILanguageServer) section key = async {
    Log.Logger.Information $"Getting {section} {key} config..."
    let! config =
        s.Configuration.GetConfiguration(ConfigurationItem(Section = configSection))
        |> Async.AwaitTask

    let configValue = config.GetSection(section).Item(key)
    let value =
        match configValue with
            | "" | null -> None
            | _ -> Some configValue

    Log.Logger.Information $"Got {key} {value}"
    return value
}

let private getWorkspaceFolder (s:ILanguageServer) =
    let workspaceFolders = s.WorkspaceFolderManager.CurrentWorkspaceFolders
    if not (Seq.isEmpty workspaceFolders) then
        let workspaceRoot = workspaceFolders |> Seq.head
        Some <| workspaceRoot.Uri.ToUri().LocalPath
    else
        None

let private onStartup definitions = OnLanguageServerStartedDelegate(fun (s:ILanguageServer) _cancellationToken ->
    async {
        let configGetter() = getConfig s configSection pathKey
        // Not sure if this is needed to ensure configuration is loaded, or allow a task/context switch
        // Either way, if it's not here, then getWorkspaceFolder returns null
        let! _ = configGetter() 
        let workspaceFolder = getWorkspaceFolder s

        let definitionsLoader = Definitions.loader definitions

        let registerWatchedFiles = Some <| WatchedFiles.register s definitionsLoader

        Definitions.init definitions s.Window.LogInfo configGetter registerWatchedFiles
        Definitions.addFolder definitions workspaceFolder
      
        definitionsLoader()

    } |> Async.StartAsTask :> Task)

let private configureServer (input: Stream) (output: Stream) (opts:LanguageServerOptions) =
    let definitions = Definitions.create()
    opts
        .WithInput(input)
        .WithOutput(output)

        .OnStarted(onStartup definitions)
        .WithConfigurationSection(configSection) // Add back in when implementing didConfigurationChanged handling
        .ConfigureLogging(fun z -> z.AddLanguageProtocolLogging().AddSerilog(Log.Logger).SetMinimumLevel(LogLevel.Trace) |> ignore)
        .WithServerInfo(ServerInfo(Name = "Contextly"))

        .OnDidChangeConfiguration(Configuration.handler <| Definitions.loader definitions)
        .OnCompletion(Completion.handler <| Definitions.find definitions <| TextDocument.getWord, Completion.registrationOptions)
        .OnHover(Hover.handler <| Definitions.find definitions <| TextDocument.getWord, Hover.registrationOptions)

        |> TextDocument.onSync

        |> ignore
     

let setupAndStartLanguageServer (input: Stream) (output: Stream) = async {
    Log.Logger.Information "Starting server..."
    let! server =
        configureServer input output
        |> LanguageServer.From
        |> Async.AwaitTask
    Log.Logger.Information "Server started."
    return server
}