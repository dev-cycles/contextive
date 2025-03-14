module Contextive.LanguageServer.FileReader

open System.IO
open Contextive.Core.File

let private tryReadFile (pathConfig: PathConfiguration) =
    if File.Exists(pathConfig.Path) then
        File.ReadAllText(pathConfig.Path) |> Ok
    else
        let fileError =
            match pathConfig with
            | { IsDefault = true } -> DefaultFileNotFound
            | { IsDefault = false } -> FileNotFound

        Error(fileError)

let private readFromPath path =
    match path with
    | Error(e) -> Error(PathInvalid(e))
    | Ok(p) ->
        { AbsolutePath = p.Path
          Contents = tryReadFile p }
        |> Ok

let reader pathGetter =
    fun () ->
        async {
            let! absolutePath = pathGetter ()
            return readFromPath absolutePath
        }

let pathReader (path: PathConfiguration) =
    path |> Ok |> readFromPath |> Result.bind (fun r -> r.Contents)
