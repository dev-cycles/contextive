module Contextive.LanguageServer.Glossary

[<Literal>]
let private GLOSSARY_FILE_GLOB = "**/*.contextive.yml"

type Logger = { info: string -> unit }

type OnWatchedFilesEventHandlers =
    { OnCreated: string -> unit
      OnChanged: string -> unit
      OnDeleted: string -> unit }

    static member Default =
        { OnChanged = fun _ -> ()
          OnCreated = fun _ -> ()
          OnDeleted = fun _ -> () }

type CreateGlossary =
    { FileScanner: string -> string list
      Log: Logger
      RegisterWatchedFiles: string -> OnWatchedFilesEventHandlers -> unit }

let create (createGlossary: CreateGlossary) =
    let glossaryFiles = createGlossary.FileScanner GLOSSARY_FILE_GLOB

    glossaryFiles
    |> Seq.iter (fun p -> createGlossary.Log.info $"Found definitions file at '{p}'...")

    createGlossary.RegisterWatchedFiles GLOSSARY_FILE_GLOB OnWatchedFilesEventHandlers.Default

    ()
