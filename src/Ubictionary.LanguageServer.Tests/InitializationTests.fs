module Ubictionary.LanguageServer.Tests.InitializationTests

open System
open System.Threading.Tasks
open Expecto
open Newtonsoft.Json.Linq
open Swensen.Unquote
open System.Collections.Generic
open OmniSharp.Extensions.JsonRpc
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Client

let private initTestClient = async {
    let testClient = new TestClient()
    return! testClient.Initialize(None) |> Async.AwaitTask
}

let private initTestClientWithConfig clientConfigBuilder = async {
    let testClient = new TestClient()
    return! testClient.Initialize(Some clientConfigBuilder) |> Async.AwaitTask
}

let private configSection values =
    let results = new List<JToken>()
    let configValue = JObject()
    results.Add(configValue)
    values |> Map.iter (fun k (v:string) ->
        configValue.[k] <- JValue(v))
    Container(results)

let private configRequestIncludesSection section (configRequest:ConfigurationParams) =
    configRequest.Items |> Seq.map (fun ci -> ci.Section) |> Seq.contains section

let private handleConfigurationRequest section configValues =
    let configSectionResult = configSection configValues
    let handler c =
        if configRequestIncludesSection section c then
            Task.FromResult(configSectionResult)
        else
            Task.FromResult(null)
    Func<ConfigurationParams, Task<Container<JToken>>>(handler)

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

            let configHandler = handleConfigurationRequest "ubictionary" (Map [("ubictionary_path", pathValue)])

            let logAwaiter = ConditionAwaiter.create()

            let logHandler (l:LogMessageParams) =
                l.Message |> ConditionAwaiter.received logAwaiter 
                Serilog.Log.Logger.Information("Received Log Message: {l}", l)
                Task.CompletedTask 

            let clientOptionsBuilder (b:LanguageClientOptions) = 
                b.OnRequest("workspace/configuration", configHandler, JsonRpcHandlerOptions())
                    .OnNotification("window/logMessage", logHandler, JsonRpcHandlerOptions())
                    .WithCapability(Capabilities.DidChangeConfigurationCapability())
                |> ignore
            use! client = clientOptionsBuilder |> initTestClientWithConfig
            test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>
            test <@ client.ClientSettings.Capabilities.Workspace.DidChangeConfiguration.IsSupported @>
            let logCondition = fun (m:string) -> m.Contains("Loading ubictionary")
            let! reply = ConditionAwaiter.waitFor logAwaiter logCondition 1500
            test <@ match reply with
                    | Some replyMsg -> replyMsg.Contains(pathValue)
                    | None -> false @>
        }

    ]
    