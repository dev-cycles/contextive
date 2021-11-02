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

let private handleConfigurationRequest section configValues (configRequested:ref<bool>) =
    let configSectionResult = configSection configValues
    let handler c =
        if configRequestIncludesSection section c then
            configRequested := true
            Task.FromResult(configSectionResult)
        else
            Task.FromResult(null)
    Func<ConfigurationParams, Task<Container<JToken>>>(handler)

type WaitForCondition = 
    {
        ReplyChannel: AsyncReplyChannel<string>
        Message: string
    }

type Command =
    | Received of String
    | WaitFor of WaitForCondition

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
            let configRequested = ref false

            let configHandler = handleConfigurationRequest "ubictionary" (Map [("ubictionary_path", pathValue)]) configRequested

            // let logAwaiter = MailboxProcessor.Start(fun inbox -> 
            //     let rec loop (conditions: WaitForCondition list) = async {
            //         let! (msg: Command) = inbox.Receive()
            //         let newState =
            //             match msg with
            //             | Received msg ->
            //                 conditions |> Seq.iter (fun c -> if msg.Contains(c.Message) then c.ReplyChannel.Reply(msg))
            //                 conditions
            //             | WaitFor waitFor -> waitFor :: conditions
            //         return! loop newState
            //     }
            //     loop [])

            // let logHandler (l:LogMessageParams) =
            //     Received (l.Message) |> logAwaiter.Post
            //     Task.CompletedTask 

            let clientOptionsBuilder (b:LanguageClientOptions) = 
                b.OnRequest("workspace/configuration", configHandler, JsonRpcHandlerOptions())
                    //.OnRequest("window/logMessage", logHandler, JsonRpcHandlerOptions())
                    .WithCapability(Capabilities.DidChangeConfigurationCapability())
                |> ignore
            use! client = clientOptionsBuilder |> initTestClientWithConfig
            test <@ client.ClientSettings.Capabilities.Workspace.Configuration.IsSupported @>
            test <@ client.ClientSettings.Capabilities.Workspace.DidChangeConfiguration.IsSupported @>
            do! Async.Sleep 5000
            //let! reply = logAwaiter.PostAndTryAsyncReply((fun r -> (WaitFor ({ReplyChannel=r; Message="Loading ubictionary"}))), 2000)
            //test <@ reply = Some "ubictionary" @>
            test <@ configRequested.Value @>
        }

    ]
    