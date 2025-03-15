module Contextive.LanguageServer.PathResolver

open System
open System.Runtime.InteropServices
open System.IO
open Contextive.Core.File

let private getProc () =
    let proc = new Diagnostics.Process()

    if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        proc.StartInfo.FileName <- "powershell"
    else
        proc.StartInfo.FileName <- "bash"
        proc.StartInfo.ArgumentList.Add "-c"

    proc

let private shellOutToGetPath wsf configuredPath =
    let proc = getProc ()
    let procCmd = $"echo \"{configuredPath}\""
    proc.StartInfo.ArgumentList.Add procCmd
    proc.StartInfo.WorkingDirectory <- defaultArg wsf null
    proc.StartInfo.RedirectStandardOutput <- true
    proc.StartInfo.RedirectStandardError <- true

    try
        match proc.Start() with
        | false ->
            let err = proc.StandardError.ReadToEnd()
            Error(err)
        | true ->
            Serilog.Log.Logger.Error "Started process."
            let loc = proc.StandardOutput.ReadToEnd().Trim()
            let err = proc.StandardError.ReadToEnd().Trim()

            match proc.WaitForExit 500 with
            | false -> Error(err)
            | true ->
                match err with
                | ""
                | null -> Ok(loc)
                | _ -> Error(err)
    with e ->
        Serilog.Log.Logger.Error $"Got {e}"
        Error(e.ToString())

let resolvePath workspaceFolder (path: PathConfiguration option) =
    match path with
    | None -> Error("No path defined - please check \"contextive.path\" setting.")
    | Some p ->
        if Path.IsPathRooted(p.Path) then
            Ok(p)
        else if p.Path.Contains("$(") then
            shellOutToGetPath workspaceFolder p.Path
            |> Result.map (fun newPath -> { p with Path = newPath })
        else
            match workspaceFolder with
            | Some wsf -> Ok({ p with Path = Path.Combine(wsf, p.Path) })
            | None -> Error($"Unable to locate path '{p.Path}' as not in a workspace.")
