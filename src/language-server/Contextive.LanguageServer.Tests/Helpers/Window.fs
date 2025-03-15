module Contextive.LanguageServer.Tests.Helpers.Window

open System
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Window
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

// Temporary, pending release including merged https://github.com/OmniSharp/csharp-language-server-protocol/issues/1117
[<CapabilityKey("Window", "ShowMessage")>]
type ShowMessageClientCapability() =
    interface ICapability

type RequestHandler<'TRequest, 'TResponse> = 'TRequest -> Threading.Tasks.Task<'TResponse>
type NotificationHandler<'TRequest> = 'TRequest -> Unit

let requestHandler<'TRequest, 'TResponse> awaiter (response: 'TResponse) : RequestHandler<'TRequest, 'TResponse> =
    fun (msg: 'TRequest) ->
        ConditionAwaiter.received awaiter msg
        Threading.Tasks.Task.FromResult(response)

let notificationHandler<'TRequest> awaiter : NotificationHandler<'TRequest> =
    fun (msg: 'TRequest) -> ConditionAwaiter.received awaiter msg

let showMessageRequestHandlerBuilder
    (handler: RequestHandler<ShowMessageRequestParams, MessageActionItem>)
    (opts: LanguageClientOptions)
    : LanguageClientOptions =
    opts
        .WithCapability(ShowMessageClientCapability())
        .OnShowMessageRequest(handler)
    |> ignore

    opts

let showMessageHandlerBuilder
    (handler: NotificationHandler<ShowMessageParams>)
    (opts: LanguageClientOptions)
    : LanguageClientOptions =
    opts.WithCapability(ShowMessageClientCapability()).OnShowMessage(handler)
    |> ignore

    opts

let showDocumentRequestHandlerBuilder
    (handler: RequestHandler<ShowDocumentParams, ShowDocumentResult>)
    (opts: LanguageClientOptions)
    : LanguageClientOptions =
    opts
        .WithCapability(ShowDocumentClientCapabilities())
        .OnShowDocument(handler)
    |> ignore

    opts
