module Ubictionary.LanguageServer.Server

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Window
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Serilog
open System.IO

let configSection = "ubictionary"
let pathKey = "path"

let getConfig section key (config:IConfiguration) =
    let configValue = config.GetSection(section).Item(key)
    match configValue with
    | "" | null -> None
    | _ -> Some configValue

let private onStartup (loadDefinitions:string -> unit) (clearDefinitions:unit -> unit) = OnLanguageServerStartedDelegate(fun (s:ILanguageServer) _cancellationToken ->
    async {
        Log.Logger.Information "Getting config..."
        let! config =
            s.Configuration.GetConfiguration(ConfigurationItem(Section = configSection))
            |> Async.AwaitTask
        let path = config |> getConfig configSection pathKey

        Log.Logger.Information $"Got path {path}"

        let workspaceFolders = s.WorkspaceFolderManager.CurrentWorkspaceFolders
        if not (Seq.isEmpty workspaceFolders) then
            let workspaceRoot = workspaceFolders |> Seq.head
            let rootPath =  workspaceRoot.Uri.ToUri().LocalPath

            match path with
            | Some p -> 
                let fullPath = System.IO.Path.Combine(rootPath, p)
                s.Window.LogInfo $"Loading ubictionary from {fullPath}"
                loadDefinitions fullPath 
            | None -> ()
        else
            clearDefinitions()

    } |> Async.StartAsTask :> Task)

let private configureServer (input: Stream) (output: Stream) (opts:LanguageServerOptions) =
    opts
        .WithInput(input)
        .WithOutput(output)

        .OnStarted(onStartup Definitions.load Definitions.clear)
        //.WithConfigurationSection(configSection) // Add back in when implementing didConfigurationChanged handling
        .ConfigureLogging(fun z -> z.AddLanguageProtocolLogging().AddSerilog(Log.Logger).SetMinimumLevel(LogLevel.Trace) |> ignore)
        .WithServerInfo(ServerInfo(Name = "Ubictionary"))

        .OnHover(Hover.handler, Hover.registrationOptions)
        .OnCompletion(Completion.handler Definitions.find, Completion.registrationOptions)
        
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