module Contextive.LanguageServer.Tests.SurveyTests

open System
open Expecto
open Swensen.Unquote
open Contextive.LanguageServer.Tests.Helpers
open Helpers.TestClient
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.JsonRpc
open System.IO
open System.Reflection
open System.Linq

type Handler<'TRequest, 'TResponse> = 'TRequest -> Threading.Tasks.Task<'TResponse>

let handler<'TRequest, 'TResponse> awaiter (response: 'TResponse) : Handler<'TRequest, 'TResponse> =
    fun (msg: 'TRequest) ->
        ConditionAwaiter.received awaiter msg
        Threading.Tasks.Task.FromResult(response)

let handlerBuilder method (handler: Handler<'TRequest, 'TResponse>) (opts: LanguageClientOptions) =
    opts.OnRequest(method, handler, JsonRpcHandlerOptions())

type HandlerBuilder<'TRequest, 'TResponse> =
    Handler<'TRequest, 'TResponse> -> LanguageClientOptions -> LanguageClientOptions

let showMessageRequestHandlerBuilder: HandlerBuilder<ShowMessageRequestParams, MessageActionItem> =
    handlerBuilder "window/showMessageRequest"

let showDocumentRequestHandlerBuilder: HandlerBuilder<ShowDocumentParams, ShowDocumentResult> =
    handlerBuilder "window/showDocument"

let latchFile = Path.Combine(System.AppContext.BaseDirectory, "survey-prompted.txt")

[<Tests>]
let initializationTests =
    testSequenced
    <| testList
        "LanguageServer.Survey Prompt Tests"
        [ testAsync "Server shows survey prompt and creates latch file, if latch-file doesn't exit" {
              let messageAwaiter = ConditionAwaiter.create ()

              File.Delete(latchFile)

              let pathValue = Guid.NewGuid().ToString()

              let config =
                  [ showMessageRequestHandlerBuilder <| handler messageAwaiter null
                    Workspace.optionsBuilder ""
                    ConfigurationSection.contextivePathOptionsBuilder pathValue ]

              let! (client, reply) = TestClient(config) |> initAndWaitForReply

              let! msg = ConditionAwaiter.waitForAny messageAwaiter 1500

              let receivedMsg = msg.Value

              test <@ receivedMsg.Message.Contains("survey") @>
              test <@ receivedMsg.Type = MessageType.Info @>
              test <@ receivedMsg.Actions.Count() = 2 @>
              test <@ receivedMsg.Actions.First().Title = "Sure, I'll help" @>
              test <@ receivedMsg.Actions.Skip(1).First().Title = "No thanks" @>
              test <@ File.Exists(latchFile) @>
              File.Delete(latchFile)
          }

          testAsync "Server does not show prompt if latch-file already exists" {
              let messageAwaiter = ConditionAwaiter.create ()

              File.Create(latchFile).Close()

              let pathValue = Guid.NewGuid().ToString()

              let config =
                  [ showMessageRequestHandlerBuilder <| handler messageAwaiter null
                    Workspace.optionsBuilder ""
                    ConfigurationSection.contextivePathOptionsBuilder pathValue ]

              let! (client, reply) = TestClient(config) |> initAndWaitForReply

              let! msg = ConditionAwaiter.waitForAny messageAwaiter 1500

              File.Delete(latchFile)

              test <@ msg.IsNone @>
          }

          testAsync "Server opens form when user says they'll help" {

              let messageAwaiter = ConditionAwaiter.create ()
              let showDocAwaiter = ConditionAwaiter.create ()

              File.Delete(latchFile)

              let pathValue = Guid.NewGuid().ToString()

              let response = MessageActionItem(Title = "Sure, I'll help")
              let showDocResponse = ShowDocumentResult(Success = true)

              let config =
                  [ showMessageRequestHandlerBuilder <| handler messageAwaiter response
                    showDocumentRequestHandlerBuilder <| handler showDocAwaiter showDocResponse
                    Workspace.optionsBuilder ""
                    ConfigurationSection.contextivePathOptionsBuilder pathValue ]

              let! (client, reply) = TestClient(config) |> initAndWaitForReply

              let! showDocMsg = ConditionAwaiter.waitForAny showDocAwaiter 1500

              File.Delete(latchFile)

              let receivedDoc = showDocMsg.Value

              test <@ receivedDoc.External.Value = true @>
              test <@ receivedDoc.Uri.ToString().Contains("forms.gle") @>

          } ]
