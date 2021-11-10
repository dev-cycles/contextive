module Ubictionary.LanguageServer.Tests.InitializationTests

open System
open System.Threading.Tasks
open Expecto
open Swensen.Unquote
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Window
open OmniSharp.Extensions.LanguageServer.Client
open TestClient
open ConfigurationSection

[<Tests>]
let initializationTests =
    testList "Initialization Tests" [

        testAsync "Can Initialize Language Server" {
            use! client = initTestClient
            test <@ not (isNull client.ClientSettings) @>
            test <@ not (isNull client.ServerSettings) @>
        }

        testAsync "Has Correct ServerInfo Name" {
            use! client = initTestClient
            test <@ client.ServerSettings.ServerInfo.Name = "Ubictionary" @>
        }

        testAsync "Server requests ubictionary file location configuration" {
            let pathValue = Guid.NewGuid().ToString()

            let logAwaiter = ConditionAwaiter.create()

            let logHandler = setupLogHandler logAwaiter

            use! client = (Map [("path", pathValue)]) |> initTestClientWithConfig logHandler id

            let! reply = waitForConfigLoaded logAwaiter

            test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>
            test <@ client.ClientSettings.Capabilities.Workspace.DidChangeConfiguration.IsSupported @>
            
            test <@ match reply with
                    | Some replyMsg -> replyMsg.Contains(pathValue)
                    | None -> false @>
        }

    ]
    