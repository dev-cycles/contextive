module Contextive.LanguageServer.Tests.E2e.GlossaryFileInitializationTests

open System
open Expecto
open Swensen.Unquote
open Contextive.LanguageServer.Tests.Helpers
open Contextive.LanguageServer.Tests.Helpers.Window
open Contextive.LanguageServer.Tests.Helpers.TestClient
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

        return (res, fileExists, contents)
    }

[<Tests>]
let tests =
    testSequencedGroup "Initialization Tests"
    <| testList
        "LanguageServer.GlossaryFile Initialization Tests"
        [ testTask "Initialization command creates default glossary file at configured path" {
              let pathValue = Guid.NewGuid().ToString()

              let! res, fileExists, contents = initializeContextive None pathValue []

              File.Delete(pathValue)

              test <@ res.Success @>
              test <@ fileExists @>

              do! Verifier.Verify("Default Glossary File", contents).ToTask()
          }

          testTask
              "Initialization command creates default glossary file at configured path when subfolder doesn't exist" {
              let pathValue = $"{Guid.NewGuid().ToString()}/{Guid.NewGuid().ToString()}"

              let! res, fileExists, contents = initializeContextive None pathValue []

              let folder = Path.GetDirectoryName(pathValue)
              Directory.EnumerateFiles(folder) |> Seq.iter File.Delete
              Directory.Delete(folder)

              test <@ res.Success @>
              test <@ fileExists @>

              do! Verifier.Verify("Default Glossary File in Non-existent Path", contents).ToTask()
          }

          testAsync "Initialization command opens new glossary file" {
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

          testAsync "Initialization command opens existing glossary file without changing it" {
              let pathValue = "existing.yml"
              let workspacePath = Path.Combine("fixtures", "initialization_tests")
              let fullPath = Path.Combine(workspacePath, pathValue)

              let showDocAwaiter = ConditionAwaiter.create ()
              let showDocResponse = ShowDocumentResult(Success = true)

              let existingContents = File.ReadAllText(fullPath)

              let! _, _, newContents =
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

          ]
