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

type TestClientConfig = {
    WorkspaceFolderPath: string
    ConfigurationSettings: Map<string, String>
}

type InitializationOptions =
    | SimpleTestClient
    | TestClient of TestClientConfig

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
            | Some b -> base.InitializeClient(Action<LanguageClientOptions>(b >> ignore))
            | _ -> base.InitializeClient(null)

let private createTestClient clientOptsBuilder = async {
    let testClient = new TestClient()
    
    return! testClient.Initialize(clientOptsBuilder) |> Async.AwaitTask
}

let private setupLogHandler logAwaiter:LogHandler =
    fun (l:LogMessageParams) ->
        l.Message |> ConditionAwaiter.received logAwaiter 
        Task.CompletedTask

let private  waitForConfigLoaded logAwaiter = async {
    let logCondition = fun (m:string) -> m.Contains("Loading ubictionary")

    return! ConditionAwaiter.waitFor logAwaiter logCondition 1500
}

let private initAndWaitForConfigLoaded testClientConfig = async {
    let logAwaiter = ConditionAwaiter.create()

    let logHandler = logAwaiter |> setupLogHandler 
    let configHandler = ConfigurationSection.createHandler "ubictionary" testClientConfig.ConfigurationSettings
    let workspaceHandler = Workspace.createHandler testClientConfig.WorkspaceFolderPath

    let clientOptionsBuilder (b:LanguageClientOptions) = 
        b.EnableWorkspaceFolders()
            .WithCapability(Capabilities.DidChangeConfigurationCapability())
            .OnConfiguration(configHandler)
            .OnLogMessage(logHandler)
            .OnWorkspaceFolders(workspaceHandler)
    
    let! client = Some clientOptionsBuilder |> createTestClient

    let! reply = waitForConfigLoaded logAwaiter

    // TODO Work out how to return the path
    //client.DefinitionPath <- reply.Value

    return client
}

let init initOptions =
    match initOptions with
    | SimpleTestClient -> createTestClient None
    | TestClient(testClientConfig) -> initAndWaitForConfigLoaded testClientConfig