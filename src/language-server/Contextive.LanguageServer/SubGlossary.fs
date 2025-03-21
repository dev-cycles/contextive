module Contextive.LanguageServer.SubGlossary

open Contextive.Core.GlossaryFile
open Contextive.Core.File
open Logger
open Globs

type StartSubGlossary =
    { Path: PathConfiguration; Log: Logger }

type Lookup =
    { Filter: Filter
      OpenFileUri: string
      Rc: AsyncReplyChannel<FindResult> }

type Message =
    | Start of StartSubGlossary
    | Reload
    | Lookup of Lookup

type FileReaderFn = PathConfiguration -> Result<string, FileError>

type State =
    { FileReader: FileReaderFn
      ImportedFiles: string seq
      GlossaryFile: GlossaryFile
      Log: Logger
      Path: PathConfiguration option }

    static member Initial =
        { FileReader = fun _ -> Error(NotYetLoaded)
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
            state.Log.info "Successfully loaded."

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
            | DefaultFileNotFound -> state.Log.info "No glossary file configured, and default file not found."
            | _ ->
                let errorMessage = fileErrorMessage fileError
                let msg = $"Error loading glossary: {errorMessage}"
                state.Log.error msg

            fileError)
        |> Result.defaultValue state

    let normalizePath (originalPath: string) newPath : string =
        let originalBase = System.IO.Path.GetDirectoryName originalPath
        System.IO.Path.Combine(originalBase, newPath)


    let rec private loadAndMergeImports (importSource: string) (state: State) =
        if state.GlossaryFile.Imports.Count > 0 then
            let import = state.GlossaryFile.Imports |> Seq.head |> normalizePath importSource

            if Seq.contains import state.ImportedFiles then
                state.Log.info $"Not loading {import} as it's already been loaded (circular dependency)."
                state
            else
                let newState =
                    loadFile state { Path = import; IsDefault = false } loadAndMergeImports

                { state with
                    GlossaryFile =
                        { state.GlossaryFile with
                            Contexts =
                                Seq.append state.GlossaryFile.Contexts newState.GlossaryFile.Contexts
                                |> ResizeArray } }
        else
            state

    let reload (state: State) =
        async {
            return
                match state.Path with
                | None -> state
                | Some p -> loadFile state p loadAndMergeImports

        }

    let start (state: State) (startSubGlossary: StartSubGlossary) =
        { state with
            Path = Some startSubGlossary.Path
            Log = startSubGlossary.Log }
        |> reload

    let lookup (state: State) (lookup: Lookup) =
        async {

            let matchOpenFileUri = matchGlobs lookup.OpenFileUri

            state.GlossaryFile.Contexts
            |> Seq.filter matchOpenFileUri
            |> lookup.Filter
            |> lookup.Rc.Reply

            return state
        }

let private handleMessage (state: State) (msg: Message) =
    async {
        return!
            match msg with
            | Start startSubGlossary -> Handlers.start state startSubGlossary
            | Reload -> Handlers.reload state
            | Lookup p -> Handlers.lookup state p
    }

let private safeFileReader fileReader p : Result<string, FileError> =
    try
        fileReader p
    with e ->
        $"Unexpected error reading file {p}: {e.ToString()}" |> ParsingError |> Error

let start fileReader (startSubGlossary: StartSubGlossary) : T =
    let subGlossary =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (state: State) =
                async {
                    let! (msg: Message) = inbox.Receive()

                    let! newState = handleMessage state msg

                    return! loop newState
                }

            loop
            <| { State.Initial with
                   FileReader = safeFileReader fileReader
                   Path = None })

    Start startSubGlossary |> subGlossary.Post

    subGlossary

let reload (subGlossary: T) = Reload |> subGlossary.Post

let lookup (subGlossary: T) openFileUri (filter: Filter) =
    subGlossary.PostAndAsyncReply(fun rc ->
        Lookup(
            { Filter = filter
              OpenFileUri = openFileUri
              Rc = rc }
        ))
