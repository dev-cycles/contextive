module Ubictionary.LanguageServer.Server

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Window
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open Microsoft.Extensions.Logging
open Serilog
open System.IO

let configSection = "ubictionary"
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

let private onStartup (id:string)= OnLanguageServerStartedDelegate(fun (s:ILanguageServer) _cancellationToken ->
    async {
        let! path = getConfig s configSection pathKey
        let workspaceFolder = getWorkspaceFolder s

        let fullPath = Definitions.load id workspaceFolder path

        match fullPath with
            | Some p -> s.Window.LogInfo $"Loading ubictionary from {p}"
            | None -> ()

    } |> Async.StartAsTask :> Task)

let private configureServer (input: Stream) (output: Stream) (opts:LanguageServerOptions) =
    let id = System.Guid.NewGuid().ToString()
    opts
        .WithInput(input)
        .WithOutput(output)

        .OnStarted(onStartup id)
        //.WithConfigurationSection(configSection) // Add back in when implementing didConfigurationChanged handling
        .ConfigureLogging(fun z -> z.AddLanguageProtocolLogging().AddSerilog(Log.Logger).SetMinimumLevel(LogLevel.Trace) |> ignore)
        .WithServerInfo(ServerInfo(Name = "Ubictionary"))

        .OnCompletion(Completion.handler <| Definitions.find id, Completion.registrationOptions)
        .OnHover(Hover.handler <| Definitions.find id, Hover.registrationOptions)
        .OnDidOpenTextDocument(TextDocument.DidOpen.handler, TextDocument.DidOpen.registrationOptions)

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