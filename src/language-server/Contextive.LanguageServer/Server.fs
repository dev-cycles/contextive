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

[<Literal>]
let defaultContextiveDefinitionsPath = ".contextive/definitions.yml"

let configSection = "contextive"
let pathKey = "path"
let assembly = Assembly.GetExecutingAssembly()
let name = assembly.GetName().Name

let version =
    assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        .InformationalVersion

let private getConfig (s: ILanguageServer) section key =
    async {
        Log.Logger.Information $"Getting {section} {key} config..."

        let! config =
            // We need to use `GetScopedConfiguration` because of https://github.com/OmniSharp/csharp-language-server-protocol/issues/1101
            // We can revert to `GetConfiguration` when that bug is fixed.
            s.Configuration.GetScopedConfiguration("file:///", System.Threading.CancellationToken.None)
            //s.Configuration.GetConfiguration(ConfigurationItem(Section = configSection))
            |> Async.AwaitTask

        let configValue = config.GetSection(section).Item(key)

        let value =
            if System.String.IsNullOrEmpty configValue then
                match key with
                | key when key = pathKey -> Some defaultContextiveDefinitionsPath
                | _ -> None
            else
                Some configValue

        Log.Logger.Information $"Got {key} {value}"
        return value
    }

let private getWorkspaceFolder (s: ILanguageServer) =
    let workspaceFolders = s.WorkspaceFolderManager.CurrentWorkspaceFolders

    if not (Seq.isEmpty workspaceFolders) then
        let workspaceRoot = workspaceFolders |> Seq.head
        Some <| workspaceRoot.Uri.ToUri().LocalPath
    else
        None

let private showSurveyPrompt (s: ILanguageServer) =
    task {
        let latchFile = Path.Combine(System.AppContext.BaseDirectory, "survey-prompted.txt")

        if not <| File.Exists(latchFile) then
            File.Create(latchFile).Close()

            let goToSurveyAction = "Sure, I'll help"
            let surveyUri = "https://forms.gle/3pJSUYmLHv5RQ1m1A"

            let surveyPrompt =
                ShowMessageRequestParams(
                    Message =
                        "Would you like to help shape the future of Contextive by completing a short (10min) survey?",
                    Type = MessageType.Info,
                    Actions =
                        Container(
                            [ MessageActionItem(Title = goToSurveyAction)
                              MessageActionItem(Title = "No thanks") ]
                        )
                )

            let! response = s.Window.ShowMessageRequest(surveyPrompt, System.Threading.CancellationToken.None)

            if response <> null && response.Title = goToSurveyAction then
                let! res = s.Window.ShowDocument(ShowDocumentParams(Uri = surveyUri, External = true))
                ()

    }

let private onStartupShowSurveyPrompt =
    OnLanguageServerStartedDelegate(fun (s: ILanguageServer) _cancellationToken -> showSurveyPrompt (s))

let private onStartupConfigureServer definitions =
    OnLanguageServerStartedDelegate(fun (s: ILanguageServer) _cancellationToken ->
        async {
            s.Window.LogInfo $"Starting {name} v{version}..."
            let configGetter () = getConfig s configSection pathKey
            // Not sure if this is needed to ensure configuration is loaded, or allow a task/context switch
            // Either way, if it's not here, then getWorkspaceFolder returns null
            let! _ = configGetter ()

            let workspaceFolder = getWorkspaceFolder s

            let definitionsLoader = Definitions.loader definitions

            let registerWatchedFiles = Some <| WatchedFiles.register s definitionsLoader

            let definitionsFileLoader =
                PathResolver.resolvePath workspaceFolder
                |> Configuration.resolvedPathGetter configGetter
                |> FileLoader.loader

            Definitions.init
                definitions
                s.Window.LogInfo
                definitionsFileLoader
                registerWatchedFiles
                s.Window.ShowWarning

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
        .OnStarted(onStartupShowSurveyPrompt)
        .WithConfigurationSection(configSection) // Add back in when implementing didConfigurationChanged handling
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
