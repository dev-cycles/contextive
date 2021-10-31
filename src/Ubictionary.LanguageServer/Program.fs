open Ubictionary.LanguageServer
open Serilog
open System

let private startWithConsole = 
    { Input = Console.OpenStandardInput(); Output = Console.OpenStandardOutput()}
    |> setupAndStartLanguageServer

let private setupLogging = 
    Log.Logger <- LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.File("log.txt")
        .CreateLogger();

[<EntryPoint>]
let main argv =
    setupLogging
    startWithConsole
    |> Async.RunSynchronously
    0 // return an integer exit code