module Ubictionary.LanguageServer.Tests.InitializationTests
open Expecto
open Swensen.Unquote

let initTestClient = async {
    let testClient = new TestClient()
    return! testClient.Initialize() |> Async.AwaitTask
}


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

    ]
    