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
            | Some f -> base.InitializeClient(Action<LanguageClientOptions>(f))
            | None -> base.InitializeClient(null)

let initTestClient = async {
    let testClient = new TestClient()
    return! testClient.Initialize(None) |> Async.AwaitTask
}

let initTestClientWithOptions clientOptionsBuilder = async {
    let testClient = new TestClient()
    return! testClient.Initialize(Some clientOptionsBuilder) |> Async.AwaitTask
}

type LogHandler = LogMessageParams -> Task
type ClientOptionsBuilder = LanguageClientOptions -> LanguageClientOptions

let setupLogHandler logAwaiter:LogHandler =
    fun (l:LogMessageParams) ->
        l.Message |> ConditionAwaiter.received logAwaiter 
        Task.CompletedTask
    
let initTestClientWithConfig (logHandler:LogHandler) (clientOptionsBuilder:ClientOptionsBuilder) config  = async {
    let configHandler = ConfigurationSection.handleConfigurationRequest "ubictionary" config

    let standardClientOptionsBuilder (b:LanguageClientOptions) = 
        (b |> clientOptionsBuilder)
            .EnableWorkspaceFolders()
            .WithCapability(Capabilities.DidChangeConfigurationCapability())
            .OnConfiguration(configHandler)
            .OnLogMessage(logHandler)
        |> ignore

    return! standardClientOptionsBuilder |> initTestClientWithOptions
}

let waitForConfigLoaded logAwaiter = async {
    let logCondition = fun (m:string) -> m.Contains("Loading ubictionary")

    return! ConditionAwaiter.waitFor logAwaiter logCondition 1500
}

let initTestClientWithConfigAndWait clientOptionsBuilder config = async {
    let logAwaiter = ConditionAwaiter.create()
    let logHandler = logAwaiter |> setupLogHandler 
    let! client = initTestClientWithConfig logHandler clientOptionsBuilder config
    do! waitForConfigLoaded logAwaiter |> Async.Ignore
    return client
}

    