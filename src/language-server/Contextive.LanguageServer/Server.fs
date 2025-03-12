module Contextive.LanguageServer.Server

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
open System.Reflection


let assembly = Assembly.GetExecutingAssembly()
let name = assembly.GetName().Name

let version =
    assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        .InformationalVersion

let private onStartupConfigureServer (glossary : Glossary.T) =
    OnLanguageServerStartedDelegate(fun (s: ILanguageServer) _cancellationToken ->
        async {
            s.Window.LogInfo $"Starting {name} v{version}..."

            let glossaryReloader = Glossary.reloader glossary

            let registerWatchedFiles = Some <| WatchedFiles.register s glossaryReloader

            let showWarning =
                if s.Window.ClientSettings.Capabilities.Window.ShowMessage.IsSupported then
                    s.Window.ShowWarning
                else
                    fun _ -> ()

            let! glossaryFileReader = DefaultGlossaryFileProvider.getDefaultGlossaryFileReader s

            Glossary.init
                glossary
                (fun m ->
                    s.Window.Log(m)
                    Serilog.Log.Logger.Information(m))
                glossaryFileReader
                registerWatchedFiles
                showWarning

            glossaryReloader ()

        }
        |> Async.StartAsTask
        :> Task)


let private configureServer (input: Stream) (output: Stream) (opts: LanguageServerOptions) =
    let glossary = Glossary.create ()

    opts
        .WithInput(input)
        .WithOutput(output)

        .OnStarted(onStartupConfigureServer glossary)
        .WithConfigurationSection(DefaultGlossaryFileProvider.ConfigSection) // Add back in when implementing didConfigurationChanged handling
        .ConfigureLogging(fun z ->
            z
                .AddLanguageProtocolLogging()
                .AddSerilog(Log.Logger)
                .SetMinimumLevel(LogLevel.Trace)
            |> ignore)
        .WithServerInfo(ServerInfo(Name = name, Version = version))

        .OnDidChangeConfiguration(Configuration.handler <| Glossary.reloader glossary)
        .OnCompletion(
            Completion.handler <| Glossary.find glossary <| TextDocument.findToken,
            Completion.registrationOptions
        )
        .OnHover(Hover.handler <| Glossary.find glossary <| TextDocument.findToken,
        Hover.registrationOptions)

    |> TextDocument.onSync

    |> ignore


let setupAndStartLanguageServer (input: Stream) (output: Stream) =
    async {
        Log.Logger.Information "Starting server..."
        let! server = configureServer input output |> LanguageServer.From |> Async.AwaitTask
        Log.Logger.Information "Server started."
        return server
    }
