module Contextive.LanguageServer.Glossary

open Contextive.Core.GlossaryFile
open Contextive.LanguageServer.Logger
open Contextive.Core.File

type OnWatchedFilesEventHandlers =
    { OnCreated: string -> unit
      OnChanged: string -> unit
      OnDeleted: string -> unit }

    static member Default =
        { OnChanged = fun _ -> ()
          OnCreated = fun _ -> ()
          OnDeleted = fun _ -> () }

type DeRegisterWatch = unit -> unit

type SubGlossaryOperations =
    { Start: NSubGlossary.StartSubGlossary -> NSubGlossary.T
      Reload: NSubGlossary.T -> unit }

// Create glossary with static dependencies
type CreateGlossary =
    { FileScanner: string -> string list
      SubGlossaryOps: SubGlossaryOperations }

// InitGlossary with dynamic dependencies that rely on the language server having already started
type InitGlossary =
    { Log: Logger
      RegisterWatchedFiles: OnWatchedFilesEventHandlers -> string option -> DeRegisterWatch
      DefaultSubGlossaryPathResolver: unit -> Async<Result<PathConfiguration, FileError>> }

type State =
    { Log: Logger
      FileScanner: string -> string list
      RegisterWatchedFiles: string option -> DeRegisterWatch
      SubGlossaries: Map<string, NSubGlossary.T>
      SubGlossaryOps: SubGlossaryOperations
      DefaultSubGlossaryPathResolver: unit -> Async<Result<PathConfiguration, FileError>>
      DefaultSubGlossary: NSubGlossary.T option
      DeRegisterDefaultSubGlossaryFileWatcher: DeRegisterWatch option }

type Lookup =
    { Filter: Filter
      Rc: AsyncReplyChannel<FindResult> }

type Message =
    | Init of InitGlossary * OnWatchedFilesEventHandlers
    | ReloadDefaultGlossaryFile
    | WatchedFileCreated of string
    | WatchedFileChanged of string
    | Lookup of Lookup

type T = MailboxProcessor<Message>

[<Literal>]
let private GLOSSARY_FILE_GLOB = "**/*.contextive.yml"

module private Handlers =

    let create (createGlossary: CreateGlossary) =

        let state =
            { FileScanner = createGlossary.FileScanner
              Log = Logger.Noop
              RegisterWatchedFiles = fun _ -> fun () -> ()
              SubGlossaries = Map []
              SubGlossaryOps = createGlossary.SubGlossaryOps
              DefaultSubGlossaryPathResolver = fun _ -> FileError.NotYetLoaded |> Error |> async.Return
              DefaultSubGlossary = None
              DeRegisterDefaultSubGlossaryFileWatcher = None }

        state

    let init (state: State) (initGlossary: InitGlossary) (watchedFileshandlers: OnWatchedFilesEventHandlers) =
        let state =
            { state with
                DefaultSubGlossaryPathResolver = initGlossary.DefaultSubGlossaryPathResolver
                RegisterWatchedFiles = initGlossary.RegisterWatchedFiles watchedFileshandlers
                Log = initGlossary.Log }

        Some GLOSSARY_FILE_GLOB |> state.RegisterWatchedFiles |> ignore

        let glossaryFiles = state.FileScanner GLOSSARY_FILE_GLOB

        glossaryFiles
        |> Seq.iter (fun p ->
            state.SubGlossaryOps.Start
                { Path = { Path = p; IsDefault = false }
                  Log = state.Log }
            |> ignore)

        state


    let watchedFileChanged (state: State) path =
        let subGlossary = state.SubGlossaries[path]
        state.SubGlossaryOps.Reload subGlossary
        state

    let watchedFileCreated (state: State) path =
        let exists = state.SubGlossaries.ContainsKey(path)

        if exists then
            watchedFileChanged state path
        else
            let subGlossary =
                state.SubGlossaryOps.Start
                    { Path = { Path = path; IsDefault = false }
                      Log = state.Log }

            let newSubGlossaries = state.SubGlossaries.Add(path, subGlossary)

            { state with
                SubGlossaries = newSubGlossaries }

    let reloadDefaultGlossaryFile (state: State) =
        async {

            match state.DeRegisterDefaultSubGlossaryFileWatcher with
            | Some deregister -> deregister ()
            | _ -> ()

            let! pathOpt = state.DefaultSubGlossaryPathResolver()

            return
                pathOpt
                |> Result.map (fun path ->
                    let defaultSubGlossary = state.SubGlossaryOps.Start { Path = path; Log = state.Log }

                    { state with
                        DefaultSubGlossary = defaultSubGlossary |> Some
                        SubGlossaries = state.SubGlossaries.Add(path.Path, defaultSubGlossary)
                        DeRegisterDefaultSubGlossaryFileWatcher = Some path.Path |> state.RegisterWatchedFiles |> Some })
                |> Result.mapError (fun fileError ->
                    let errorMessage = fileErrorMessage fileError
                    let msg = $"Error loading glossary: {errorMessage}"
                    state.Log.error msg)
                |> Result.defaultValue state
        }

    let lookup (state: State) (lookup: Lookup) =
        async {
            match state.DefaultSubGlossary with
            | Some subGlossary ->
                let! result = NSubGlossary.lookup subGlossary lookup.Filter
                lookup.Rc.Reply(result)
            | None -> lookup.Rc.Reply(Seq.empty)

            return state
        }

let private handleMessage (state: State) (msg: Message) =
    async {
        return!
            match msg with
            | Init(initGlossary, watchedFileHandlers) ->
                Handlers.init state initGlossary watchedFileHandlers |> async.Return
            | ReloadDefaultGlossaryFile -> Handlers.reloadDefaultGlossaryFile state
            | WatchedFileCreated path -> Handlers.watchedFileCreated state path |> async.Return
            | WatchedFileChanged path -> Handlers.watchedFileChanged state path |> async.Return
            | Lookup lookup -> Handlers.lookup state lookup
    }

let watchedFileHandlers (glossary: T) =
    { OnWatchedFilesEventHandlers.Default with
        OnCreated = fun p -> glossary.Post(WatchedFileCreated(p))
        OnChanged = fun p -> glossary.Post(WatchedFileChanged(p)) }

let create (createGlossary: CreateGlossary) : T =
    MailboxProcessor.Start(fun inbox ->
        let rec loop (state: State) =
            async {
                let! (msg: Message) = inbox.Receive()

                let! newState = handleMessage state msg

                return! loop newState
            }

        loop <| Handlers.create createGlossary)

let init (glossary: T) (initGlossary: InitGlossary) =
    Init(initGlossary, watchedFileHandlers glossary) |> glossary.Post

let reloadDefaultGlossaryFile (glossary: T) () =
    ReloadDefaultGlossaryFile |> glossary.Post

let lookup (glossary: T) (_: string) (filter: Filter) =
    glossary.PostAndAsyncReply(fun rc -> Lookup { Filter = filter; Rc = rc })
