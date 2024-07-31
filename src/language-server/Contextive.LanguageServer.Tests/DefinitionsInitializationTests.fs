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

let private schemaFileName = "contextive-schema.json"

let initializeContextive
    (workspaceFolderOpt: string option)
    (pathValue: string)
    (extraConfig: ClientOptionsBuilder list)
    =
    async {
        let workspacePath = Option.defaultValue "" workspaceFolderOpt

        let config =
            [ Workspace.optionsBuilder <| workspacePath
              ConfigurationSection.contextivePathBuilder pathValue ]
            @ extraConfig

        use! client = TestClient(config) |> init

        let responseRouterReturns = client.SendRequest("contextive/initialize")

        let! res =
            responseRouterReturns.Returning<InitResult>(System.Threading.CancellationToken.None)
            |> Async.AwaitTask

        let fullPath = Path.Combine(workspacePath, pathValue)
        let fileExists = File.Exists(fullPath)
        let contents = File.ReadAllText(fullPath)
        let schemaPath = Path.GetDirectoryName(fullPath)
        let schemaFullPath = Path.Combine(schemaPath, schemaFileName)
        let schemaContents = File.ReadAllText(schemaFullPath)

        return (res, fileExists, contents, schemaContents)
    }

[<Tests>]
let tests =
    testSequencedGroup "Initialization Tests"
    <| testList
        "LanguageServer.Definitions Initialization Tests"
        [ testAsync "Initialization command creates default definitions file at configured path" {
              let pathValue = Guid.NewGuid().ToString()

              let! res, fileExists, contents, schemaContents = initializeContextive None pathValue []

              File.Delete(pathValue)

              test <@ res.Success @>
              test <@ fileExists @>

              do!
                  Verifier.Verify("Default Definitions", contents)
                  |> Async.AwaitTask
                  |> Async.Ignore

              do!
                  Verifier.Verify("Default Definitions Schema", schemaContents)
                  |> Async.AwaitTask
                  |> Async.Ignore
          }

          testAsync
              "Initialization command creates default definitions file at configured path when subfolder doesn't exist" {
              let pathValue = $"{Guid.NewGuid().ToString()}/{Guid.NewGuid().ToString()}"

              let! res, fileExists, contents, schemaContents = initializeContextive None pathValue []

              let folder = Path.GetDirectoryName(pathValue)
              Directory.EnumerateFiles(folder) |> Seq.iter File.Delete
              Directory.Delete(folder)

              test <@ res.Success @>
              test <@ fileExists @>

              do!
                  Verifier.Verify("Default Definitions in Non-existent Path", contents)
                  |> Async.AwaitTask
                  |> Async.Ignore

              do!
                  Verifier.Verify("Default Definitions Schema in Non-existent Path", schemaContents)
                  |> Async.AwaitTask
                  |> Async.Ignore
          }

          testAsync "Initialization command opens new definitions file" {
              let pathValue = Guid.NewGuid().ToString()

              let showDocAwaiter = ConditionAwaiter.create ()
              let showDocResponse = ShowDocumentResult(Success = true)

              do!
                  initializeContextive
                      None
                      pathValue
                      [ showDocumentRequestHandlerBuilder
                        <| requestHandler showDocAwaiter showDocResponse ]
                  |> Async.Ignore

              File.Delete(pathValue)
              File.Delete(schemaFileName)

              let! showDocMsg = ConditionAwaiter.waitForAny showDocAwaiter

              test <@ showDocMsg.Value.Uri.ToString().Contains(pathValue) @>
          }

          testAsync "Initialization command opens existing definitions file without changing it" {
              let pathValue = "existing.yml"
              let workspacePath = Path.Combine("fixtures", "initialization_tests")
              let fullPath = Path.Combine(workspacePath, pathValue)

              let showDocAwaiter = ConditionAwaiter.create ()
              let showDocResponse = ShowDocumentResult(Success = true)

              let existingContents = File.ReadAllText(fullPath)

              let! _, _, newContents, _ =
                  initializeContextive
                      (Some workspacePath)
                      pathValue
                      [ showDocumentRequestHandlerBuilder
                        <| requestHandler showDocAwaiter showDocResponse ]

              let! showDocMsg = ConditionAwaiter.waitForAny showDocAwaiter

              File.WriteAllText(fullPath, existingContents)

              test <@ showDocMsg.Value.Uri.ToString().Contains(pathValue) @>

              test <@ newContents = existingContents @>
          }

          testAsync "Initialization command updates existing schema file" {
              let pathValue = "existing.yml"
              let workspacePath = Path.Combine("fixtures", "initialization_tests")
              let schemaPath = Path.Combine(workspacePath, schemaFileName)

              let existingSchemaContents = File.ReadAllText(schemaPath)
              File.WriteAllText(schemaPath, "Temporary Schema Contents")

              let! _, _, _, schemaContents = initializeContextive (Some workspacePath) pathValue []

              test <@ schemaContents = existingSchemaContents @>
          }

          ]
