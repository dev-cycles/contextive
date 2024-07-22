module Contextive.LanguageServer.Tests.InitializationTests

open System
open Expecto
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open Swensen.Unquote
open Contextive.LanguageServer.Tests.Helpers
open Helpers.TestClient

[<Tests>]
let tests =
    testList
        "LanguageServer.Initialization Tests"
        [

          testAsync "Can Initialize Language Server" {
              use! client = SimpleTestClient |> init
              test <@ not (isNull client.ClientSettings) @>
              test <@ not (isNull client.ServerSettings) @>
          }

          testAsync "Has Correct ServerInfo Name" {
              use! client = SimpleTestClient |> init
              test <@ client.ServerSettings.ServerInfo.Name = "Contextive.LanguageServer" @>
          }

          testAsync "Server loads contextive file from relative location with workspace" {
              let pathValue = Guid.NewGuid().ToString()

              let config =
                  [ Workspace.optionsBuilder ""
                    ConfigurationSection.contextivePathBuilder pathValue ]

              let! (client, reply, _) = TestClient(config) |> initAndWaitForReply
              use client = client

              test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>

              test <@ (defaultArg reply "").Contains(pathValue) @>
          }

          testAsync "Server loads contextive file from relative location with rootUi and no workspace" {
              let pathValue = Guid.NewGuid().ToString()

              let config =
                  [ Workspace.rootOptionsBuilder ""
                    ConfigurationSection.contextivePathBuilder pathValue ]

              let! (client, reply, _) = TestClient(config) |> initAndWaitForReply
              use client = client

              test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>

              test <@ (defaultArg reply "").Contains(pathValue) @>
          }

          testAsync "Server loads contextive file from absolute location without workspace" {
              let pathValue = Guid.NewGuid().ToString()

              let config = [ ConfigurationSection.contextivePathBuilder $"/tmp/{pathValue}" ]

              let! (client, reply, _) = TestClient(config) |> initAndWaitForReply
              use client = client

              test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>

              test <@ (defaultArg reply "").Contains(pathValue) @>
          }

          testAsync "Server does NOT load contextive file from relative location without workspace" {
              let pathValue = Guid.NewGuid().ToString()
              let config = [ ConfigurationSection.contextivePathBuilder pathValue ]

              let! (client, reply, _) = TestClientWithCustomInitWait(config, Some pathValue) |> initAndWaitForReply
              use client = client

              test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>

              test
                  <@
                      reply = Some
                          $"Error loading definitions: Invalid Path: Unable to locate path '{pathValue}' as not in a workspace."
                  @>
          }

          testAsync "Server loads contextive file from default location when no configuration supplied and file exists" {
              let config =
                  [ Workspace.optionsBuilder "fixtures/default_tests"
                    ConfigurationSection.configurationHandlerBuilder "dummySection" (fun () -> Map []) ]

              let expectedResult = Some "Successfully loaded."

              let! (client, reply, _) = TestClientWithCustomInitWait(config, expectedResult) |> initAndWaitForReply
              use client = client

              test <@ reply = expectedResult @>
          }
          
          testAsync "Server doesn't error when no configuration supplied but file doesn't exists" {
              let showErrorAwaiter = ConditionAwaiter.create ()
              
              let config =
                  [ Workspace.optionsBuilder "fixtures/non_existent_folder_tests"
                    ConfigurationSection.configurationHandlerBuilder "dummySection" (fun () -> Map [])
                    Window.showMessageHandlerBuilder <| Window.notificationHandler<ShowMessageParams> showErrorAwaiter
                    ]

              let expectedResult = Some "No definitions file configured, and default file not found."

              let! (client, reply, _) = TestClientWithCustomInitWait(config, expectedResult) |> initAndWaitForReply
              use client = client
              
              let! receivedMessage = ConditionAwaiter.waitForAnyTimeout 1 showErrorAwaiter

              test <@ reply = expectedResult @>
              test <@ receivedMessage.IsNone @>
          }
          
          testAsync "Server errors when configuration supplied and file doesn't exists" {
              let showErrorAwaiter = ConditionAwaiter.create ()
              let pathValue = Guid.NewGuid().ToString()
              
              let config =
                  [ Workspace.optionsBuilder "fixtures/non_existent_folder_tests"
                    ConfigurationSection.contextivePathBuilder pathValue
                    Window.showMessageHandlerBuilder <| Window.notificationHandler<ShowMessageParams> showErrorAwaiter
                    ]

              let expectedResult = Some "Error loading definitions: Definitions file not found."

              let! (client, reply, _) = TestClientWithCustomInitWait(config, expectedResult) |> initAndWaitForReply
              use client = client
              
              let! receivedMessage = ConditionAwaiter.waitForAny showErrorAwaiter

              test <@ reply = expectedResult @>
              test <@ receivedMessage.Value.Type = MessageType.Warning @>
              test <@ receivedMessage.Value.Message = expectedResult.Value @>
          }

          ]
