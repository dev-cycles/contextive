namespace Ubictionary.LanguageServer.Tests

open System.IO.Pipelines
open OmniSharp.Extensions.LanguageProtocol.Testing
open OmniSharp.Extensions.JsonRpc.Testing
open Ubictionary.LanguageServer.Server

type TestClient() =
    inherit LanguageServerTestBase(JsonRpcTestOptions())

    override _.SetupServer() =
        let clientPipe = Pipe()
        let serverPipe = Pipe()
        setupAndStartLanguageServer (serverPipe.Reader.AsStream()) (clientPipe.Writer.AsStream())
            |> Async.Ignore
            |> Async.Start
        (clientPipe.Reader.AsStream(), serverPipe.Writer.AsStream())

    member _.Initialize() =
        base.InitializeClient(null)