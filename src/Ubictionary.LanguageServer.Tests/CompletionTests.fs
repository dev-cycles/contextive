module Ubictionary.LanguageServer.Tests.CompletionTests

open Expecto
open Swensen.Unquote
open System.Threading.Tasks
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Window
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

        testAsync "Given ubictionary, respond with valid completion list " {

            let configHandler = ConfigurationSection.handleConfigurationRequest "ubictionary" (Map [("path", "definitions.yml")])
            let workspaceFolderHandler (p:WorkspaceFolderParams) =
                Task.FromResult(Container(WorkspaceFolder(Uri = DocumentUri.FromFileSystemPath(
                    Path.Combine(Directory.GetCurrentDirectory(),"fixtures", "simple_ubictionary")
                ))))

            let logAwaiter = ConditionAwaiter.create()
            let logHandler (l:LogMessageParams) =
                l.Message |> ConditionAwaiter.received logAwaiter 
                Task.CompletedTask 

            let clientOptionsBuilder (b:LanguageClientOptions) = 
                b.EnableWorkspaceFolders()
                    .WithCapability(Capabilities.DidChangeConfigurationCapability())
                    .OnConfiguration(configHandler)
                    .OnLogMessage(logHandler)
                    .OnWorkspaceFolders(workspaceFolderHandler)
                |> ignore

            use! client = clientOptionsBuilder |> initTestClientWithConfig

            let logCondition = fun (m:string) -> m.Contains("Loading ubictionary")
            let! reply = ConditionAwaiter.waitFor logAwaiter logCondition 1500

            let completionParams = CompletionParams()

            let! result = client.TextDocument.RequestCompletion(completionParams).AsTask() |> Async.AwaitTask

            let completionLabels = result.Items |> Seq.map (fun x -> x.Label)

            test <@ (completionLabels, ["firstTerm";"secondTerm";"thirdTerm"]) ||> Seq.compareWith compare = 0 @>
        }
    ]