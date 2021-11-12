module Ubictionary.LanguageServer.Tests.InitializationTests

open System
open Expecto
open Swensen.Unquote
open TestClient

[<Tests>]
let initializationTests =
    testList "Initialization Tests" [

        testAsync "Can Initialize Language Server" {
            use! client = Simple |> init
            test <@ not (isNull client.ClientSettings) @>
            test <@ not (isNull client.ServerSettings) @>
        }

        testAsync "Has Correct ServerInfo Name" {
            use! client = Simple |> init
            test <@ client.ServerSettings.ServerInfo.Name = "Ubictionary" @>
        }

        testAsync "Server requests ubictionary file location configuration" {
            let pathValue = Guid.NewGuid().ToString()

            let logAwaiter = ConditionAwaiter.create()
            let logHandler = setupLogHandler logAwaiter

            let config = Map [("path", pathValue)]

            use! client =  Config(config, logHandler) |> init

            let! reply = waitForConfigLoaded logAwaiter

            test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>
            test <@ client.ClientSettings.Capabilities.Workspace.DidChangeConfiguration.IsSupported @>
            
            test <@ match reply with
                    | Some replyMsg -> replyMsg.Contains(pathValue)
                    | None -> false @>
        }

    ]
    