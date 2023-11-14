module Contextive.LanguageServer.Tests.PathResolverTests

open Expecto
open Contextive.LanguageServer
open Swensen.Unquote
open System.Runtime.InteropServices

[<Tests>]
let tests =
    testList
        "LanguageServer.Path Resolver Tests"
        [ testCase "No Workspace, non-root path"
          <| fun () ->
              let p = PathResolver.resolvePath None (Some "path/to/file")
              test <@ p = Error("Unable to locate path 'path/to/file' as not in a workspace.") @>

          testCase "No Workspace, no path"
          <| fun () ->
              let p = PathResolver.resolvePath None None
              test <@ p = Error("No path defined - please check \"contextive.path\" setting.") @>

          testCase "Workspace, non-root path"
          <| fun () ->
              let p = PathResolver.resolvePath (Some "/workspace") (Some "path")
              test <@ p = Ok(System.IO.Path.Join("/workspace", "path")) @>

          testCase "No Workspace, Root path"
          <| fun () ->
              let p = PathResolver.resolvePath None (Some "/path")
              test <@ p = Ok("/path") @>

          testCase "Workspace, Root path"
          <| fun () ->
              let p = PathResolver.resolvePath (Some "/workspace") (Some "/path")
              test <@ p = Ok("/path") @>

          testCase "No Workspace, Process Shell path"
          <| fun () ->
              let p = PathResolver.resolvePath None (Some "$(echo \"/workspace\")/path")
              test <@ p = Ok("/workspace/path") @>

          testCase "Workspace, Process Shell path"
          <| fun () ->
              let p = PathResolver.resolvePath None (Some "$(echo \"/workspace\")/path")
              test <@ p = Ok("/workspace/path") @>

          testCase "Process Error"
          <| fun () ->
              let p = PathResolver.resolvePath None (Some "$(noprogram)/path")

              test
                  <@
                      match p with
                      | Error errorMsg ->
                          if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
                              errorMsg.Contains(
                                  "noprogram : The term 'noprogram' is not recognized as the name of a cmdlet, function, script file"
                              )
                          else
                              errorMsg.Contains("noprogram: command not found")
                      | _ -> false
                  @> ]
