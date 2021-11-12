module Ubictionary.LanguageServer.Tests.InitializationTests

open System
open Expecto
open Swensen.Unquote
open TestClient

[<Tests>]
let initializationTests =
    testList "Initialization Tests" [

        testAsync "Can Initialize Language Server" {
            use! client = SimpleTestClient |> init
            test <@ not (isNull client.ClientSettings) @>
            test <@ not (isNull client.ServerSettings) @>
        }

        testAsync "Has Correct ServerInfo Name" {
            use! client = SimpleTestClient |> init
            test <@ client.ServerSettings.ServerInfo.Name = "Ubictionary" @>
        }

        testAsync "Server requests ubictionary file location configuration" {
            let pathValue = Guid.NewGuid().ToString()
            let config = {
                WorkspaceFolderPath = ""
                ConfigurationSettings = Map [("path", pathValue)]
            }

            let! (client, reply) = TestClient(config) |> initWithReply

            test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>
            test <@ client.ClientSettings.Capabilities.Workspace.DidChangeConfiguration.IsSupported @>
            
            test <@ (defaultArg reply "").Contains(pathValue) @>
        }

    ]
    