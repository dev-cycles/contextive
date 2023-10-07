module Contextive.LanguageServer.Tests.InitializationTests

open System
open Expecto
open Swensen.Unquote
open Contextive.LanguageServer.Tests.Helpers
open Helpers.TestClient

[<Tests>]
let initializationTests =
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
                    ConfigurationSection.contextivePathOptionsBuilder pathValue ]

              let! (client, reply) = TestClient(config) |> initAndWaitForReply

              test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>
              test <@ client.ClientSettings.Capabilities.Workspace.DidChangeConfiguration.IsSupported @>

              test <@ (defaultArg reply "").Contains(pathValue) @>
          }

          testAsync "Server loads contextive file from absolute location without workspace" {
              let pathValue = Guid.NewGuid().ToString()

              let config =
                  [ ConfigurationSection.contextivePathOptionsBuilder $"/tmp/{pathValue}" ]

              let! (client, reply) = TestClient(config) |> initAndWaitForReply

              test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>
              test <@ client.ClientSettings.Capabilities.Workspace.DidChangeConfiguration.IsSupported @>

              test <@ (defaultArg reply "").Contains(pathValue) @>
          }

          testAsync "Server does NOT load contextive file from relative location without workspace" {
              let pathValue = Guid.NewGuid().ToString()
              let config = [ ConfigurationSection.contextivePathOptionsBuilder pathValue ]

              let! (client, reply) = TestClientWithCustomInitWait(config, Some pathValue) |> initAndWaitForReply

              test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>
              test <@ client.ClientSettings.Capabilities.Workspace.DidChangeConfiguration.IsSupported @>

              test
                  <@
                      reply = Some
                          $"Error loading definitions: Unable to locate path '{pathValue}' as not in a workspace."
                  @>
          }

          testAsync "Server loads contextive file from default location when no configuration supplied" {
              let config =
                  [ Workspace.optionsBuilder ""
                    ConfigurationSection.optionsBuilder "dummySection" (fun () -> Map []) ]

              let defaultPath = ".contextive/definitions.yml"

              let! (client, reply) = TestClient(config) |> initAndWaitForReply

              test <@ (defaultArg reply "").Contains(defaultPath) @>
          }

          ]
