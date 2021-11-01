module Ubictionary.LanguageServer.LanguageServer

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open Serilog
open System.IO
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

let private onInitializeEvent msg server request response _cancellationToken = 
    Log.Logger.Information $"Ubictionary: {msg} {server} {request} {response}"
    Task.CompletedTask
let private onInitialize s r = onInitializeEvent "Initializing" s r None
let private onInitialized = onInitializeEvent "Initialized"
let private onStarted s = onInitializeEvent "Started" s None None

let configureServer (input: Stream) (output: Stream) (opts:LanguageServerOptions) =
    opts
        .WithInput(input)
        .WithOutput(output)
        .ConfigureLogging(fun b -> 
            b.AddLanguageProtocolLogging()
                .AddSerilog(Log.Logger)
                .SetMinimumLevel(LogLevel.Trace)
                |> ignore)
        .OnInitialize(OnLanguageServerInitializeDelegate(onInitialize))
        .OnInitialized(OnLanguageServerInitializedDelegate(onInitialized))
        .OnStarted(OnLanguageServerStartedDelegate(onStarted))
        .WithServerInfo(ServerInfo(Name = "Ubictionary"))
        .WithServices(fun x -> 
            x.AddLogging(fun b -> 
                b.SetMinimumLevel(LogLevel.Trace) |> ignore)
            |> ignore)
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