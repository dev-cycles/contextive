module Contextive.LanguageServer.Tests.DefinitionsInitializationTests

open System
open Expecto
open Swensen.Unquote
open Contextive.LanguageServer.Tests.Helpers
open Contextive.LanguageServer.Tests.Helpers.Window
open Helpers.TestClient
open System.IO
open VerifyExpecto
open OmniSharp.Extensions.LanguageServer.Protocol.Models

[<CLIMutable>]
type InitResult = { Success: bool }

[<Tests>]
let tests =
    testSequencedGroup "Initialization Tests"
    <| testList
        "LanguageServer.Definitions Initialization Tests"
        [ testAsync "Initialization command creates default definitions file at configured path" {
              let pathValue = Guid.NewGuid().ToString()

              let config =
                  [ Workspace.optionsBuilder ""
                    ConfigurationSection.contextivePathBuilder pathValue ]

              let! (client, _) = TestClient(config) |> initAndWaitForReply

              let responseRouterReturns = client.SendRequest("contextive/initialize")

              let! res =
                  responseRouterReturns.Returning<InitResult>(System.Threading.CancellationToken.None)
                  |> Async.AwaitTask

              let fileExists = File.Exists(pathValue)

              let contents = File.ReadAllText(pathValue)

              File.Delete(pathValue)

              test <@ res.Success @>
              test <@ fileExists @>

              do!
                  Verifier.Verify("Default Definitions", contents)
                  |> Async.AwaitTask
                  |> Async.Ignore
          }

          testAsync "Initialization command opens new definitions file" {
              let pathValue = Guid.NewGuid().ToString()

              let showDocAwaiter = ConditionAwaiter.create ()
              let showDocResponse = ShowDocumentResult(Success = true)

              let config =
                  [ Workspace.optionsBuilder ""
                    ConfigurationSection.contextivePathBuilder pathValue
                    showDocumentRequestHandlerBuilder <| handler showDocAwaiter showDocResponse ]

              let! (client, _) = TestClient(config) |> initAndWaitForReply

              let responseRouterReturns = client.SendRequest("contextive/initialize")

              do!
                  responseRouterReturns.Returning<InitResult>(System.Threading.CancellationToken.None)
                  |> Async.AwaitTask
                  |> Async.Ignore

              File.Delete(pathValue)

              let! showDocMsg = ConditionAwaiter.waitForAny showDocAwaiter 5000

              test <@ showDocMsg.Value.Uri.ToString().Contains(pathValue) @>
          }

          testAsync "Initialization command opens existing definitions file without changing it" {
              let pathValue = "existing.yml"
              let workspacePath = Path.Combine("fixtures", "initialization_tests")
              let fullPath = Path.Combine(workspacePath, pathValue)

              let showDocAwaiter = ConditionAwaiter.create ()
              let showDocResponse = ShowDocumentResult(Success = true)

              let config =
                  [ Workspace.optionsBuilder <| workspacePath
                    ConfigurationSection.contextivePathBuilder pathValue
                    showDocumentRequestHandlerBuilder <| handler showDocAwaiter showDocResponse ]

              let existingContents = File.ReadAllText(fullPath)

              let! (client, _) = TestClient(config) |> initAndWaitForReply

              let responseRouterReturns = client.SendRequest("contextive/initialize")

              do!
                  responseRouterReturns.Returning<InitResult>(System.Threading.CancellationToken.None)
                  |> Async.AwaitTask
                  |> Async.Ignore

              let! showDocMsg = ConditionAwaiter.waitForAny showDocAwaiter 3000

              let newContents = File.ReadAllText(fullPath)

              File.WriteAllText(fullPath, existingContents)

              test <@ showDocMsg.Value.Uri.ToString().Contains(pathValue) @>

              test <@ newContents = existingContents @>
          } ]
