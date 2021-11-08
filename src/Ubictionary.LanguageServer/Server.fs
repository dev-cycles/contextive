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
    config.GetSection(section).Item(key)

let private requestConfig = OnLanguageServerStartedDelegate(fun (s:ILanguageServer) _cancellationToken ->
    async {
        Log.Logger.Information "Getting config..."
        let! config =
            s.Configuration.GetConfiguration(ConfigurationItem(Section = configSection))
            |> Async.AwaitTask
        let path = config |> getConfig configSection pathKey
        Log.Logger.Information $"Got path {path}"
        s.Window.LogInfo $"Loading ubictionary from {path}"
    } |> Async.StartAsTask :> Task)

let private configureServer (input: Stream) (output: Stream) (opts:LanguageServerOptions) =
    opts
        .WithInput(input)
        .WithOutput(output)
        .OnStarted(requestConfig)
        //.WithConfigurationSection(configSection) // Add back in when implementing didConfigurationChanged handling
        .ConfigureLogging(fun z -> z.AddLanguageProtocolLogging().AddSerilog(Log.Logger).SetMinimumLevel(LogLevel.Trace) |> ignore)
        // .WithServices(fun s -> s.AddLogging(fun b -> b.SetMinimumLevel(LogLevel.Trace) |> ignore) |> ignore)
        .WithServerInfo(ServerInfo(Name = "Ubictionary"))
        .OnHover(Hover.handler, Hover.registrationOptions)
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