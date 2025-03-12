module Contextive.LanguageServer.Server

open Contextive.Core.File
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

let private onStartupConfigureServer (definitions : Definitions.T) =
    OnLanguageServerStartedDelegate(fun (s: ILanguageServer) _cancellationToken ->
        async {
            s.Window.LogInfo $"Starting {name} v{version}..."

            let definitionsLoader = Definitions.loader definitions

            let registerWatchedFiles = Some <| WatchedFiles.register s definitionsLoader

            let showWarning =
                if s.Window.ClientSettings.Capabilities.Window.ShowMessage.IsSupported then
                    s.Window.ShowWarning
                else
                    fun _ -> ()

            let! definitionsFileLoader = DefaultDefinitionsProvider.getDefaultDefinitionsFileLoader s

            Definitions.init
                definitions
                (fun m ->
                    s.Window.Log(m)
                    Serilog.Log.Logger.Information(m))
                definitionsFileLoader
                registerWatchedFiles
                showWarning

            definitionsLoader ()

        }
        |> Async.StartAsTask
        :> Task)


let private configureServer (input: Stream) (output: Stream) (opts: LanguageServerOptions) =
    let definitions = Definitions.create ()

    opts
        .WithInput(input)
        .WithOutput(output)

        .OnStarted(onStartupConfigureServer definitions)
        .WithConfigurationSection(DefaultDefinitionsProvider.ConfigSection) // Add back in when implementing didConfigurationChanged handling
        .ConfigureLogging(fun z ->
            z
                .AddLanguageProtocolLogging()
                .AddSerilog(Log.Logger)
                .SetMinimumLevel(LogLevel.Trace)
            |> ignore)
        .WithServerInfo(ServerInfo(Name = name, Version = version))

        .OnDidChangeConfiguration(Configuration.handler <| Definitions.loader definitions)
        .OnCompletion(
            Completion.handler <| Definitions.find definitions <| TextDocument.findToken,
            Completion.registrationOptions
        )
        .OnHover(Hover.handler <| Definitions.find definitions <| TextDocument.findToken, Hover.registrationOptions)

    |> TextDocument.onSync

    |> ignore


let setupAndStartLanguageServer (input: Stream) (output: Stream) =
    async {
        Log.Logger.Information "Starting server..."
        let! server = configureServer input output |> LanguageServer.From |> Async.AwaitTask
        Log.Logger.Information "Server started."
        return server
    }
