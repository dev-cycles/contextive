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

module private Startup =
    open GlossaryManager

    let onStartupConfigureServer (glossaryManager: GlossaryManager.T) =
        OnLanguageServerStartedDelegate(fun (s: ILanguageServer) _cancellationToken ->
            async {
                s.Window.LogInfo $"Starting {name} v{version}..."

                let! defaultGlossaryPathResolver = DefaultGlossaryFileProvider.getDefaultGlossaryFilePathResolver s

                let fileScanner = Configuration.getWorkspaceFolders s |> FileScanner.fileScanner

                { FileScanner = fileScanner
                  DefaultGlossaryPathResolver = defaultGlossaryPathResolver
                  Log = Logger.forLanguageServer s
                  RegisterWatchedFiles = WatchedFiles.register s }
                |> init glossaryManager

                reloadDefaultGlossaryFile glossaryManager ()

            }
            |> Async.StartAsTask
            :> Task)

let private configureServer (input: Stream) (output: Stream) (opts: LanguageServerOptions) =
    let glossaryManager =
        GlossaryManager.create
        <| { GlossaryOps =
               { Start = Glossary.start FileReader.configuredReader
                 Reload = Glossary.reload } }

    opts
        .WithInput(input)
        .WithOutput(output)

        .OnStarted(Startup.onStartupConfigureServer glossaryManager)
        .WithConfigurationSection(DefaultGlossaryFileProvider.ConfigSection) // Add back in when implementing didConfigurationChanged handling
        .ConfigureLogging(fun z ->
            z
                .AddLanguageProtocolLogging()
                .AddSerilog(Log.Logger)
                .SetMinimumLevel(LogLevel.Trace)
            |> ignore)
        .WithServerInfo(ServerInfo(Name = name, Version = version))

        .OnDidChangeConfiguration(
            Configuration.handler
            <| GlossaryManager.reloadDefaultGlossaryFile glossaryManager
        )
        .OnCompletion(
            Completion.handler
            <| GlossaryManager.lookup glossaryManager
            <| TextDocument.findToken,
            Completion.registrationOptions
        )
        .OnHover(
            Hover.handler
            <| GlossaryManager.lookup glossaryManager
            <| TextDocument.findToken,
            Hover.registrationOptions
        )

    |> TextDocument.onSync

    |> ignore


let setupAndStartLanguageServer (input: Stream) (output: Stream) =
    async {
        Log.Logger.Information "Starting server..."
        let! server = configureServer input output |> LanguageServer.From |> Async.AwaitTask
        Log.Logger.Information "Server started."
        return server
    }
