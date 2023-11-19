module Contextive.LanguageServer.Tests.Helpers.Window

open System
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Window
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

type Handler<'TRequest, 'TResponse> = 'TRequest -> Threading.Tasks.Task<'TResponse>

let handler<'TRequest, 'TResponse> awaiter (response: 'TResponse) : Handler<'TRequest, 'TResponse> =
    fun (msg: 'TRequest) ->
        ConditionAwaiter.received awaiter msg
        Threading.Tasks.Task.FromResult(response)

let showMessageRequestHandlerBuilder
    (handler: Handler<ShowMessageRequestParams, MessageActionItem>)
    (opts: LanguageClientOptions)
    : LanguageClientOptions =
    opts
        // .WithCapability(new ShowMessageRequestClientCapabilities()) // See https://github.com/OmniSharp/csharp-language-server-protocol/issues/1117
        .OnShowMessageRequest(handler)
    |> ignore

    opts

let showDocumentRequestHandlerBuilder
    (handler: Handler<ShowDocumentParams, ShowDocumentResult>)
    (opts: LanguageClientOptions)
    : LanguageClientOptions =
    opts
        .WithCapability(new ShowDocumentClientCapabilities())
        .OnShowDocument(handler)
    |> ignore

    opts
