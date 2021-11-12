module Ubictionary.LanguageServer.Tests.TestClient

open System
open System.IO.Pipelines
open System.Threading.Tasks
open OmniSharp.Extensions.LanguageProtocol.Testing
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Window
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.JsonRpc.Testing
open Ubictionary.LanguageServer.Server

type ClientOptionsBuilder = LanguageClientOptions -> LanguageClientOptions
type LogHandler = LogMessageParams -> Task
type InitializationOptions =
    | Simple
    | Options of ClientOptionsBuilder
    | ConfigAndWait of ClientOptionsBuilder * Map<string, string>
    | Config of Map<string, string> * LogHandler

type private TestClient() =
    inherit LanguageServerTestBase(JsonRpcTestOptions())

    override _.SetupServer() =
        let clientPipe = Pipe()
        let serverPipe = Pipe()
        setupAndStartLanguageServer (serverPipe.Reader.AsStream()) (clientPipe.Writer.AsStream())
            |> Async.Ignore
            |> Async.Start
        (clientPipe.Reader.AsStream(), serverPipe.Writer.AsStream())

    member _.Initialize clientOptsBuilder =
        match clientOptsBuilder with
            | Options b -> base.InitializeClient(Action<LanguageClientOptions>(b >> ignore))
            | _ -> base.InitializeClient(null)

let private initWithOptions initOptionsBuilder = async {
    let testClient = new TestClient()
    return! testClient.Initialize(initOptionsBuilder) |> Async.AwaitTask
}

let private initWithConfig (logHandler:LogHandler) (clientOptionsBuilder:ClientOptionsBuilder) config  = async {
    let configHandler = ConfigurationSection.handleConfigurationRequest "ubictionary" config

    let standardClientOptionsBuilder (b:LanguageClientOptions) = 
        (b |> clientOptionsBuilder)
            .EnableWorkspaceFolders()
            .WithCapability(Capabilities.DidChangeConfigurationCapability())
            .OnConfiguration(configHandler)
            .OnLogMessage(logHandler)
            |> ignore
        b

    return! Options(standardClientOptionsBuilder) |> initWithOptions
}

let setupLogHandler logAwaiter:LogHandler =
    fun (l:LogMessageParams) ->
        l.Message |> ConditionAwaiter.received logAwaiter 
        Task.CompletedTask

let waitForConfigLoaded logAwaiter = async {
    let logCondition = fun (m:string) -> m.Contains("Loading ubictionary")

    return! ConditionAwaiter.waitFor logAwaiter logCondition 1500
}

let private initWithConfigAndWait clientOptionsBuilder config = async {
    let logAwaiter = ConditionAwaiter.create()
    let logHandler = logAwaiter |> setupLogHandler 
    let! client = initWithConfig logHandler clientOptionsBuilder config
    do! waitForConfigLoaded logAwaiter |> Async.Ignore
    return client
}

let init initOptions =
    match initOptions with
    | Simple | Options(_) -> initWithOptions initOptions
    | ConfigAndWait(builder, config) -> initWithConfigAndWait builder config
    | Config(config, logHandler) -> initWithConfig logHandler id config
    
    