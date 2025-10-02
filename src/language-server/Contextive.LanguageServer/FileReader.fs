module Contextive.LanguageServer.FileReader

open Contextive.Core.File

type FileReader = PathConfiguration -> Result<string, FileError>

let read (localReader: FileReader) (remoteReader: FileReader) (path: PathConfiguration) =
    let reader =
        if System.Uri.IsWellFormedUriString(path.Path, System.UriKind.Absolute) then
            remoteReader
        else
            localReader

    reader path
    

let configuredReader = read LocalFileReader.read RemoteFileReader.read
