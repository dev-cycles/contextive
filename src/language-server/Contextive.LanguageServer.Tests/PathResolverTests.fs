module Contextive.LanguageServer.Tests.PathResolverTests

open Expecto
open Contextive.LanguageServer
open Contextive.Core.File
open Swensen.Unquote
open System.Runtime.InteropServices

let private pathOf p =
    (Result.defaultValue (configuredPath "") p).Path

let private sep = System.IO.Path.DirectorySeparatorChar

[<Tests>]
let tests =
    testList
        "LanguageServer.Path Resolver Tests"
        [ testCase "No Workspace, non-root path"
          <| fun () ->
              let p = PathResolver.resolvePath None ("path/to/file" |> configuredPath |> Some)
              test <@ p = Error "Unable to locate path 'path/to/file' as not in a workspace." @>

          testCase "No Workspace, no path"
          <| fun () ->
              let p = PathResolver.resolvePath None None
              test <@ p = Error "No path defined - please check \"contextive.path\" setting." @>

          testCase "Workspace, non-root path"
          <| fun () ->
              let p =
                  PathResolver.resolvePath (Some "/workspace") ("path" |> configuredPath |> Some)

              test <@ pathOf p = System.IO.Path.Join($"{sep}workspace", "path") @>

          testCase "Workspace, mixed separator paths"
          <| fun () ->
              let p =
                  "./path/to/folder"
                  |> configuredPath
                  |> Some
                  |> PathResolver.resolvePath (Some "C:\\path\\to\\workspace")

              test <@ pathOf p = $"C:\\path\\to\\workspace{sep}.{sep}path{sep}to{sep}folder" @>

          testCase "No Workspace, Root path"
          <| fun () ->
              let p = PathResolver.resolvePath None ("/path" |> configuredPath |> Some)
              test <@ pathOf p = $"{sep}path" @>

          testCase "Workspace, Root path"
          <| fun () ->
              let p =
                  PathResolver.resolvePath (Some "/workspace") ("/path" |> configuredPath |> Some)

              test <@ pathOf p = $"{sep}path" @>

          testCase "No Workspace, Process Shell path"
          <| fun () ->
              let p =
                  PathResolver.resolvePath None ("$(echo \"/workspace\")/path" |> configuredPath |> Some)

              test <@ pathOf p = $"{sep}workspace{sep}path" @>

          testCase "Workspace, Process Shell path"
          <| fun () ->
              let p =
                  PathResolver.resolvePath None ("$(echo \"/workspace\")/path" |> configuredPath |> Some)

              test <@ pathOf p = $"{sep}workspace{sep}path" @>

          testCase "Process Error"
          <| fun () ->
              let p =
                  PathResolver.resolvePath None ("$(noprogram)/path" |> configuredPath |> Some)

              test
                  <@
                      match p with
                      | Error errorMsg ->
                          (if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
                               "noprogram : The term 'noprogram' is not recognized as the name of a cmdlet, function, script file"
                           else
                               "noprogram: command not found")
                          |> errorMsg.Contains
                      | _ -> false
                  @> ]
