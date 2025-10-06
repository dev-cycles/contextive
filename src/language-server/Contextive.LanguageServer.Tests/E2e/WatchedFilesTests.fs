module Contextive.LanguageServer.Tests.E2e.WatchedFilesTests

open System
open Expecto
open Swensen.Unquote
open System.IO
open System.Linq
open System.Collections.Generic
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open Contextive.LanguageServer.Tests.Helpers
open Contextive.LanguageServer.Tests.Helpers.TestClient

let didChangeWatchedFiles (client: ILanguageClient) (uri: string) =
    client.SendNotification(
        WorkspaceNames.DidChangeWatchedFiles,
        DidChangeWatchedFilesParams(Changes = Container(FileEvent(Uri = uri, Type = FileChangeType.Created)))
    )

[<Tests>]
let tests =
    testList
        "LanguageServer.Watched File Tests"
        [

          let watcherIsForFile (fileName: string) (watchers: IEnumerable<FileSystemWatcher>) =
              watchers.Any(fun watcher ->
                  watcher.GlobPattern.Pattern.Contains(fileName)
                  && watcher.Kind = WatchKind.Change + WatchKind.Create)

          let isAbsoluteRegistration =
              function
              | WatchedFiles.Registered(_, opts) ->
                  opts.Watchers.Any(fun w -> not <| w.GlobPattern.Pattern.Contains("**"))
              | _ -> false

          testAsync "Server registers to receive watched file changes" {
              let registrationAwaiter = ConditionAwaiter.create ()

              let config =
                  [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.contextivePathBuilder "one.yml"
                    WatchedFiles.optionsBuilder registrationAwaiter ]

              use! client = TestClient(config) |> init

              let! registrationMsg = ConditionAwaiter.waitFor registrationAwaiter isAbsoluteRegistration

              test <@ client.ClientSettings.Capabilities.Workspace.DidChangeWatchedFiles.IsSupported @>

              test <@ registrationMsg.IsSome @>

              match registrationMsg with
              | Some(WatchedFiles.Registered(_, opts)) -> test <@ opts.Watchers |> watcherIsForFile "one.yml" @>
              | _ -> test <@ false @>
          }

          let serverRegistersToReceiveWatchedFileChangesOnNewConfig (newDefinitionsFile: string) =
              let newDefinitionsFileLabel = newDefinitionsFile.Replace(".", "_")

              testAsync $"when config changes to {newDefinitionsFileLabel}" {
                  let registrationAwaiter = ConditionAwaiter.create ()
                  let mutable definitionsFile = "one.yml"
                  let pathLoader () : obj = definitionsFile

                  let config =
                      [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathLoaderBuilder pathLoader
                        WatchedFiles.optionsBuilder registrationAwaiter ]

                  let! client, logAwaiter = TestClient config |> initAndGetLogAwaiter
                  use client = client

                  let! initialRegistrationMsg = ConditionAwaiter.waitFor registrationAwaiter isAbsoluteRegistration

                  ConditionAwaiter.clear registrationAwaiter

                  definitionsFile <- newDefinitionsFile
                  do! ConfigurationSection.didChangePath client definitionsFile logAwaiter

                  let! secondRegistrationMsg =
                      ConditionAwaiter.waitFor registrationAwaiter (fun m ->
                          match m with
                          | WatchedFiles.Registered _ -> true
                          | _ -> false)

                  let! unregistrationMsg =
                      ConditionAwaiter.waitFor registrationAwaiter (fun m ->
                          match m with
                          | WatchedFiles.Unregistered _ -> true
                          | _ -> false)

                  match secondRegistrationMsg with
                  | Some(WatchedFiles.Registered(_, opts)) ->
                      test <@ opts.Watchers |> watcherIsForFile newDefinitionsFile @>
                  | _ -> failtest "no registration to watch after config changed"

                  match initialRegistrationMsg, unregistrationMsg with
                  | Some(WatchedFiles.Registered(regoId, _)), Some(WatchedFiles.Unregistered(unregoId)) ->
                      test <@ regoId = unregoId @>
                  | _ -> failtest "registration of initial config not unregistered"
              }

          [ "three.yml"; "nonexistentfile.yml" ]
          |> List.map serverRegistersToReceiveWatchedFileChangesOnNewConfig
          |> testList "Server registers to receive watched file changes on new config"

          testAsync "Server reloads when the contextive file is created" {
              let newTerm = "aNewTerm"

              let relativePath = Path.Combine("fixtures", "completion_tests")
              let definitionsFile = $"{Guid.NewGuid()}.yml"

              let config =
                  [ Workspace.optionsBuilder relativePath
                    ConfigurationSection.contextivePathBuilder definitionsFile ]

              use! client = TestClient(config) |> init

              let definitionsFileUri =
                  Path.Combine(Directory.GetCurrentDirectory(), relativePath, definitionsFile)

              if File.Exists(definitionsFileUri) then
                  File.Delete(definitionsFileUri)

              didChangeWatchedFiles client definitionsFileUri

              let! labelsWhileEmpty = Completion.getCompletionLabels client

              File.WriteAllText(
                  definitionsFileUri,
                  $"""contexts:
  - terms:
    - name: {newTerm}"""
              )

              didChangeWatchedFiles client definitionsFileUri
              let! labelsAfterDefinitionsAdded = Completion.getCompletionLabels client

              File.Delete(definitionsFileUri)

              test <@ Seq.isEmpty labelsWhileEmpty @>
              test <@ labelsAfterDefinitionsAdded |> Seq.contains newTerm @>
          }

          testAsync "Server reloads when the contextive file changes" {
              let newTerm = "newterm"

              let relativePath = Path.Combine("fixtures", "completion_tests")
              let definitionsFile = "three.yml"

              let config =
                  [ Workspace.optionsBuilder relativePath
                    ConfigurationSection.contextivePathBuilder definitionsFile ]

              use! client = TestClient(config) |> init

              let definitionsFileUri =
                  Path.Combine(Directory.GetCurrentDirectory(), relativePath, definitionsFile)

              let existingContents = File.ReadAllText(definitionsFileUri)

              let newDefinition = $"{Environment.NewLine}      - name: {newTerm}"

              File.AppendAllText(definitionsFileUri, newDefinition)

              didChangeWatchedFiles client definitionsFileUri

              let! labels = Completion.getCompletionLabels client

              File.WriteAllText(definitionsFileUri, existingContents)

              test <@ labels |> Seq.contains newTerm @>
          }

          // Workaround for IntelliJ bug https://github.com/dev-cycles/contextive/issues/112
          testAsync "Server doesn't error when a didChangedWatchedFiles occurs for a non-existent file" {
              let showErrorAwaiter = ConditionAwaiter.create ()

              let relativePath = Path.Combine("fixtures", "completion_tests")

              let config =
                  [ Workspace.optionsBuilder relativePath
                    Window.showMessageHandlerBuilder
                    <| Window.notificationHandler<ShowMessageParams> showErrorAwaiter ]

              let! client, logAwaiter = TestClient config |> initAndGetLogAwaiter
              use _ = client

              let definitionsFileUri =
                  Path.Combine(Directory.GetCurrentDirectory(), relativePath, Guid.NewGuid().ToString())
                  |> DocumentUri.FromFileSystemPath
                  |> _.ToUri()
                  |> _.LocalPath

              let expectedResult =
                  $"Error loading glossary file '{definitionsFileUri}': Watched Glossary file not found."

              didChangeWatchedFiles client definitionsFileUri

              let! reply = "Error loading glossary file" |> ServerLog.waitForLogMessage logAwaiter.Value
              let! receivedMessage = ConditionAwaiter.waitForAnyTimeout 100 showErrorAwaiter

              test <@ reply.Value = expectedResult @>
              test <@ receivedMessage.IsNone @>
          } ]
