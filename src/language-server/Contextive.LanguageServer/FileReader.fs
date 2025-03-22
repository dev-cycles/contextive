module Contextive.LanguageServer.FileReader

open Contextive.Core.File

type FileReader = string -> Result<string, FileError>

let read (localReader: FileReader) (remoteReader: FileReader) (path: PathConfiguration) =
    let reader =
        if System.Uri.IsWellFormedUriString(path.Path, System.UriKind.Absolute) then
            remoteReader
        else
            localReader

    reader path.Path
    |> Result.mapError (fun e ->
        match e, path.IsDefault with
        | FileNotFound, true -> DefaultFileNotFound
        | err, _ -> err)

let configuredReader = read LocalFileReader.read RemoteFileReader.read
