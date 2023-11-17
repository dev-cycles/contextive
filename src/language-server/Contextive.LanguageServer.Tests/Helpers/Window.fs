module Contextive.LanguageServer.Tests.Helpers.Window

open System
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.JsonRpc
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

type Handler<'TRequest, 'TResponse> = 'TRequest -> Threading.Tasks.Task<'TResponse>

let handler<'TRequest, 'TResponse> awaiter (response: 'TResponse) : Handler<'TRequest, 'TResponse> =
    fun (msg: 'TRequest) ->
        ConditionAwaiter.received awaiter msg
        Threading.Tasks.Task.FromResult(response)

let handlerBuilder
    method
    (capability: ICapability option)
    (handler: Handler<'TRequest, 'TResponse>)
    (opts: LanguageClientOptions)
    =
    opts.OnRequest(method, handler, JsonRpcHandlerOptions()) |> ignore

    match capability with
    | None -> opts
    | Some c -> opts.WithCapability(c)

type HandlerBuilder<'TRequest, 'TResponse> =
    Handler<'TRequest, 'TResponse> -> LanguageClientOptions -> LanguageClientOptions

let showMessageRequestHandlerBuilder: HandlerBuilder<ShowMessageRequestParams, MessageActionItem> =
    handlerBuilder "window/showMessageRequest" None

let showDocumentRequestHandlerBuilder: HandlerBuilder<ShowDocumentParams, ShowDocumentResult> =
    handlerBuilder "window/showDocument" (Some(new ShowDocumentClientCapabilities(Support = true)))
