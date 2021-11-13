module Ubictionary.LanguageServer.Tests.TestClient

open System
open System.IO.Pipelines
open OmniSharp.Extensions.LanguageProtocol.Testing
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.JsonRpc.Testing
open Ubictionary.LanguageServer.Server

type ClientOptionsBuilder = LanguageClientOptions -> LanguageClientOptions

type InitializationOptions =
    | SimpleTestClient
    | TestClient of ClientOptionsBuilder list

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
    
    let! client = testClient.Initialize(clientOptsBuilder) |> Async.AwaitTask

    return client, None
}

let private initAndWaitForConfigLoaded testClientConfig = async {
    let logAwaiter = ConditionAwaiter.create()

    let allBuilders = ServerLog.optionsBuilder logAwaiter :: testClientConfig

    let clientOptionsBuilder = List.fold (>>) id allBuilders
    
    let! (client, _) = Some clientOptionsBuilder |> createTestClient

    let! reply = "Loading ubictionary" |> ServerLog.waitForLogMessage logAwaiter

    return (client, reply)
}

let initWithReply initOptions = async {
    return! match initOptions with
            | SimpleTestClient -> createTestClient None
            | TestClient(testClientConfig) -> initAndWaitForConfigLoaded testClientConfig
}

let init o = async {
    let! result = initWithReply o
    return fst result
}