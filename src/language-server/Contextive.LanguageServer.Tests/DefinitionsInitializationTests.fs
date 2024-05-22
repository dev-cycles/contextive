module Contextive.LanguageServer.Tests.DefinitionsInitializationTests

open System
open Expecto
open Swensen.Unquote
open Contextive.LanguageServer.Tests.Helpers
open Contextive.LanguageServer.Tests.Helpers.Window
open Helpers.TestClient
open System.IO
open VerifyExpecto
open OmniSharp.Extensions.LanguageServer.Protocol.Models

[<CLIMutable>]
type InitResult = { Success: bool }

let initializeContextive (workspaceFolderOpt:string option) (pathValue:string) (extraConfig: ClientOptionsBuilder list) = async {
        let workspacePath = Option.defaultValue "" workspaceFolderOpt
        let config =
          [ Workspace.optionsBuilder <| workspacePath
            ConfigurationSection.contextivePathBuilder pathValue ] @ extraConfig

        use! client = TestClient(config) |> init

        let responseRouterReturns = client.SendRequest("contextive/initialize")

        let! res =
          responseRouterReturns.Returning<InitResult>(System.Threading.CancellationToken.None)
          |> Async.AwaitTask
          
        let fullPath = Path.Combine(workspacePath, pathValue)
        let fileExists = File.Exists(fullPath)
        let contents = File.ReadAllText(fullPath)
        
        return (res, fileExists, contents)
    }

[<Tests>]
let tests =
    testSequencedGroup "Initialization Tests"
    <| testList
        "LanguageServer.Definitions Initialization Tests"
        [ testAsync "Initialization command creates default definitions file at configured path" {
              let pathValue = Guid.NewGuid().ToString()

              let! res, fileExists, contents = initializeContextive None pathValue []

              File.Delete(pathValue)

              test <@ res.Success @>
              test <@ fileExists @>

              do!
                  Verifier.Verify("Default Definitions", contents)
                  |> Async.AwaitTask
                  |> Async.Ignore
          }
        
          testAsync "Initialization command creates default definitions file at configured path when subfolder doesn't exist" {
              let pathValue = $"{Guid.NewGuid().ToString()}/{Guid.NewGuid().ToString()}"

              let! res, fileExists, contents = initializeContextive None pathValue []

              File.Delete(pathValue)
              Directory.Delete(Path.GetDirectoryName(pathValue))

              test <@ res.Success @>
              test <@ fileExists @>

              do!
                  Verifier.Verify("Default Definitions in Non-existent Path", contents)
                  |> Async.AwaitTask
                  |> Async.Ignore
          }

          testAsync "Initialization command opens new definitions file" {
              let pathValue = Guid.NewGuid().ToString()

              let showDocAwaiter = ConditionAwaiter.create ()
              let showDocResponse = ShowDocumentResult(Success = true)

              do! initializeContextive
                    None
                    pathValue
                    [ showDocumentRequestHandlerBuilder <| handler showDocAwaiter showDocResponse ]
                |> Async.Ignore

              File.Delete(pathValue)

              let! showDocMsg = ConditionAwaiter.waitForAny showDocAwaiter

              test <@ showDocMsg.Value.Uri.ToString().Contains(pathValue) @>
          }

          testAsync "Initialization command opens existing definitions file without changing it" {
              let pathValue = "existing.yml"
              let workspacePath = Path.Combine("fixtures", "initialization_tests")
              let fullPath = Path.Combine(workspacePath, pathValue)

              let showDocAwaiter = ConditionAwaiter.create ()
              let showDocResponse = ShowDocumentResult(Success = true)

              // let config =
              //     [ Workspace.optionsBuilder <| workspacePath
              //       ConfigurationSection.contextivePathBuilder pathValue
              //       showDocumentRequestHandlerBuilder <| handler showDocAwaiter showDocResponse ]

              let existingContents = File.ReadAllText(fullPath)
              
              let! _, _, newContents =
                    initializeContextive
                        (Some workspacePath)
                        pathValue
                        [ showDocumentRequestHandlerBuilder <| handler showDocAwaiter showDocResponse ]

              //use! client = TestClient(config) |> init

              //let responseRouterReturns = client.SendRequest("contextive/initialize")

              // do!
              //     responseRouterReturns.Returning<InitResult>(System.Threading.CancellationToken.None)
              //     |> Async.AwaitTask
              //     |> Async.Ignore

              let! showDocMsg = ConditionAwaiter.waitForAny showDocAwaiter

              //let newContents = File.ReadAllText(fullPath)

              File.WriteAllText(fullPath, existingContents)

              test <@ showDocMsg.Value.Uri.ToString().Contains(pathValue) @>

              test <@ newContents = existingContents @>
          } ]
