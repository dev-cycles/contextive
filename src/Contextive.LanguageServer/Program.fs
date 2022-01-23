module Contextive.LanguageServer.Program

open Contextive.LanguageServer.Server
open Serilog
open System

let setupLogging = 
#if DEBUG
    Log.Logger <- LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.File("log.txt", rollingInterval = RollingInterval.Day)
        .CreateLogger();
#else
    ()
#endif

let private startWithConsole = 
    setupAndStartLanguageServer (Console.OpenStandardInput()) (Console.OpenStandardOutput())

let startAndWaitForExit = async {
    let! server = startWithConsole
    do! server.WaitForExit |> Async.AwaitTask
    Log.Logger.Information "Server exited."
}

[<EntryPoint>]
let main argv =
    setupLogging
    startAndWaitForExit
    |> Async.RunSynchronously
    0 // return an integer exit code