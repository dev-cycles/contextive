module Contextive.LanguageServer.Tests.Helpers.TestClient

open System
open System.IO.Pipelines
open OmniSharp.Extensions.LanguageProtocol.Testing
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.JsonRpc.Testing
open Contextive.LanguageServer.Server

type ClientOptionsBuilder = LanguageClientOptions -> LanguageClientOptions

type InitializationOptions =
    | SimpleTestClient
    | TestClient of ClientOptionsBuilder list
    | TestClientWithCustomInitWait of ClientOptionsBuilder list * string option

type private TestClient() =
    inherit LanguageServerTestBase(JsonRpcTestOptions())

    member _.AddToDisposable languageServer = base.Disposable.Add(languageServer)

    override this.SetupServer() =
        let clientPipe = Pipe()
        let serverPipe = Pipe()

        async {
            let! _ = setupAndStartLanguageServer (serverPipe.Reader.AsStream()) (clientPipe.Writer.AsStream())
            // this.AddToDisposable server
            ()
        }
        |> Async.StartImmediate

        (clientPipe.Reader.AsStream(), serverPipe.Writer.AsStream())

    member _.Initialize clientOptsBuilder =
        match clientOptsBuilder with
        | Some b -> base.InitializeClient(Action<LanguageClientOptions>(b >> ignore))
        | _ -> base.InitializeClient(null)

let private createTestClient clientOptsBuilder =
    async {
        let testClient = new TestClient()

        let! client = testClient.Initialize(clientOptsBuilder) |> Async.AwaitTask

        return client, None, None
    }

let private initAndWaitForConfigLoaded testClientConfig (loadMessage: string option) =
    async {
        let logAwaiter = ConditionAwaiter.create ()

        let allBuilders = ServerLog.optionsBuilder logAwaiter :: testClientConfig

        let clientOptionsBuilder = List.fold (>>) id allBuilders

        let! client, _, _ = Some clientOptionsBuilder |> createTestClient

        let! reply =
            (defaultArg loadMessage "Loading contextive")
            |> ServerLog.waitForLogMessage logAwaiter

        ConditionAwaiter.clear logAwaiter

        return (client, reply, Some logAwaiter)
    }

let initAndWaitForReply initOptions =
    async {
        return!
            match initOptions with
            | SimpleTestClient -> createTestClient None
            | TestClient(testClientConfig) -> initAndWaitForConfigLoaded testClientConfig None
            | TestClientWithCustomInitWait(testClientConfig, loadMessage) ->
                initAndWaitForConfigLoaded testClientConfig loadMessage
    }

let initAndGetLogAwaiter o =
    async {
        let! client, _, logAwaiter = initAndWaitForReply o
        return client, logAwaiter
    }


let init o =
    async {
        let! client, _ = initAndGetLogAwaiter o
        return client
    }
