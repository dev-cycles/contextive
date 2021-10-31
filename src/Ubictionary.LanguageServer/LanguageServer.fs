namespace Ubictionary

module LanguageServer = 
    open System.IO
    open System.Threading.Tasks
    open OmniSharp.Extensions.LanguageServer.Server
    open OmniSharp.Extensions.LanguageServer.Protocol.Server
    open Serilog
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.Extensions.Logging

    let private onInitializeEvent msg server request response _cancellationToken = 
        Log.Logger.Information $"f# {msg} {server} {request} {response}"
        Task.CompletedTask
    let private onInitialize s r = onInitializeEvent "Initializing" s r None
    let private onInitialized = onInitializeEvent "Initialized"
    let private onStarted s = onInitializeEvent "Started" s None None

    type IOOptions = { Input: Stream; Output: Stream }

    let buildOptions ioOpts (lspOpts:LanguageServerOptions) =
        lspOpts
            .WithInput(ioOpts.Input)
            .WithOutput(ioOpts.Output)
            .ConfigureLogging(fun b -> 
                b.AddLanguageProtocolLogging()
                    .AddSerilog(Log.Logger)
                    .SetMinimumLevel(LogLevel.Trace)
                    |> ignore)
            .OnInitialize(OnLanguageServerInitializeDelegate(onInitialize))
            .OnInitialized(OnLanguageServerInitializedDelegate(onInitialized))
            .OnStarted(OnLanguageServerStartedDelegate(onStarted))
            .WithServices(fun x -> 
                x.AddLogging(fun b -> 
                    b.SetMinimumLevel(LogLevel.Trace) |> ignore)
                |> ignore)
            |> ignore

    let setupAndStartLanguageServer ioOpts = async {
        Log.Logger.Information "Starting server..."
        let! server =
            buildOptions ioOpts
            |> LanguageServer.From
            |> Async.AwaitTask
        Log.Logger.Information "Server started."
        do! server.WaitForExit |> Async.AwaitTask
        Log.Logger.Information "Server exited."
    }