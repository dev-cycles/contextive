module Contextive.LanguageServer.Glossary

open Contextive.Core.GlossaryFile
open Contextive.Core.File
open Logger
open Globs

type StartGlossary =
    { Path: PathConfiguration; Log: Logger }

type Lookup =
    { Filter: Filter
      OpenFileUri: string
      Rc: AsyncReplyChannel<FindResult> }

type Message =
    | Start of StartGlossary
    | Reload
    | Lookup of Lookup
    | Stop

type FileReaderFn = PathConfiguration -> Result<string, FileError>

type State =
    { FileReader: FileReaderFn
      ImportedFiles: string seq
      GlossaryFile: GlossaryFile
      Log: Logger
      Path: PathConfiguration option }

    static member Initial =
        { FileReader = fun _ -> Error NotYetLoaded
          ImportedFiles = Seq.empty
          GlossaryFile = GlossaryFile.Default
          Log = Logger.Noop
          Path = None }

type T = MailboxProcessor<Message>

module Handlers =

    let private loadFile state (p: PathConfiguration) loadAndMergeImports =
        state.Log.info $"Loading contextive from {p.Path}..."
        let fileContents = state.FileReader p

        fileContents
        |> Result.bind deserialize
        |> Result.map (fun r ->
            state.Log.info $"Successfully loaded from {p.Path}."

            { state with
                GlossaryFile = r
                ImportedFiles =
                    seq {
                        yield! state.ImportedFiles
                        yield p.Path
                    } })
        |> Result.map (loadAndMergeImports p.Path)
        |> Result.mapError (fun fileError ->
            match fileError with
            | FileNotFound Default -> state.Log.info "No glossary file configured, and default file not found."
            | _ ->
                let errorMessage = fileErrorMessage fileError
                let msg = $"Error loading glossary file '{p.Path}': {errorMessage}."
                state.Log.info msg

                match fileError with
                | FileNotFound Discovered -> ()
                | _ -> state.Log.error msg

            fileError)
        |> Result.defaultValue state

    let normalizePath (originalPath: string) newPath : string =
        if System.Uri.IsWellFormedUriString(newPath, System.UriKind.Absolute) then
            newPath
        else
            let originalBase = System.IO.Path.GetDirectoryName originalPath
            System.IO.Path.Combine(originalBase, newPath)

    let private loadAndMergeImport loadAndMergeImports (state: State) import =
        if Seq.contains import state.ImportedFiles then
            state.Log.info $"Not loading {import} as it's already been loaded (circular dependency)."
            state
        else
            let newState =
                loadFile state { Path = import; Source = Imported } loadAndMergeImports

            { state with
                GlossaryFile =
                    { state.GlossaryFile with
                        Contexts =
                            Seq.append state.GlossaryFile.Contexts newState.GlossaryFile.Contexts
                            |> ResizeArray } }

    let rec private loadAndMergeImports (importSource: string) (state: State) =
        state.GlossaryFile.Imports
        |> Seq.map (normalizePath importSource)
        |> Seq.fold (loadAndMergeImport loadAndMergeImports) state

    let reload (state: State) =
        async {
            return
                match state.Path with
                | None -> state
                | Some p -> loadFile state p loadAndMergeImports
                |> Some

        }

    let start (state: State) (startGlossary: StartGlossary) =
        { state with
            Path = Some startGlossary.Path
            Log = startGlossary.Log }
        |> reload

    let lookup (state: State) (lookup: Lookup) =
        async {

            let matchOpenFileUri = matchGlobs lookup.OpenFileUri

            state.GlossaryFile.Contexts
            |> Seq.filter matchOpenFileUri
            |> lookup.Filter
            |> lookup.Rc.Reply

            return Some state
        }

let private handleMessage (state: State) (msg: Message) =
    async {
        return!
            match msg with
            | Start startGlossary -> Handlers.start state startGlossary
            | Reload -> Handlers.reload state
            | Lookup p -> Handlers.lookup state p
            | Stop -> async.Return None
    }

let private safeFileReader fileReader p : Result<string, FileError> =
    try
        fileReader p
    with e ->
        $"Unexpected error reading file {p}: {e.ToString()}" |> ParsingError |> Error

let start fileReader (startGlossary: StartGlossary) : T =
    let glossary =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (state: State) =
                async {
                    let! (msg: Message) = inbox.Receive()

                    let! newState = handleMessage state msg

                    return!
                        match newState with
                        | Some ns -> loop ns
                        | None -> async.Return()
                }

            loop
            <| { State.Initial with
                   FileReader = safeFileReader fileReader
                   Path = None })

    Start startGlossary |> glossary.Post

    glossary

let stop (glossary: T) = glossary.Post Stop

let reload (glossary: T) = Reload |> glossary.Post

let lookup (glossary: T) openFileUri (filter: Filter) =
    glossary.PostAndAsyncReply(fun rc ->
        Lookup(
            { Filter = filter
              OpenFileUri = openFileUri
              Rc = rc }
        ))
