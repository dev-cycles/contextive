module Contextive.LanguageServer.FileLoader

open System.IO

type File = 
    {
        AbsolutePath: string
        Contents: Result<string, string>
    }

let private tryReadFile path =
    if File.Exists(path) then
      File.ReadAllText(path) |> Ok
    else
      Error("Definitions file not found.")

let private loadFromPath path =
    match path with
    | Error(e) -> Error(e)
    | Ok(p) -> 
        {
            AbsolutePath = p
            Contents = tryReadFile p
        } |> Ok

let loader pathGetter = fun () -> async {
        let! absolutePath = pathGetter()
        return loadFromPath absolutePath
    }
       