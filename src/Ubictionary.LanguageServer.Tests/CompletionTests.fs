module Ubictionary.LanguageServer.Tests.CompletionTests

open Expecto
open Swensen.Unquote
open System.Threading.Tasks
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Client
open TestClient

[<Tests>]
let completionTests =
    testList "Completion Tests" [
        testAsync "Given no ubictionary respond with empty completion list " {
            use! client = initTestClient

            let completionParams = CompletionParams()

            let! result = client.TextDocument.RequestCompletion(completionParams).AsTask() |> Async.AwaitTask
        
            test <@ Seq.length result.Items = 0 @>
        }

        testAsync "Given ubictionary, respond with empty completion list " {

            let configHandler = ConfigurationSection.handleConfigurationRequest "ubictionary" (Map [("path", "definitions.yml")])
            let workspaceFolderHandler (p:WorkspaceFolderParams) =
                Task.FromResult(Container(WorkspaceFolder(Uri = DocumentUri.FromFileSystemPath(
                    Path.Combine(Directory.GetCurrentDirectory(),"fixtures", "simple_ubictionary")
                ))))


            let clientOptionsBuilder (b:LanguageClientOptions) = 
                b.EnableWorkspaceFolders()
                    .WithCapability(Capabilities.DidChangeConfigurationCapability())
                    .OnConfiguration(configHandler)
                    .OnWorkspaceFolders(workspaceFolderHandler)
                |> ignore

            use! client = clientOptionsBuilder |> initTestClientWithConfig

            let completionParams = CompletionParams()

            let! result = client.TextDocument.RequestCompletion(completionParams).AsTask() |> Async.AwaitTask

            let completionLabels = result.Items |> Seq.map (fun x -> x.Label)

            test <@ (completionLabels, ["firstTerm";"secondTerm";"thirdTerm"]) ||> Seq.compareWith compare = 0 @>
        }
    ]