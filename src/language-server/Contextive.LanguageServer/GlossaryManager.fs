module Contextive.LanguageServer.GlossaryManager

open Contextive.Core.GlossaryFile
open Contextive.LanguageServer.Logger
open Contextive.Core.File
open FSharp.Control

type OnWatchedFilesEventHandlers =
    { OnCreated: string -> unit
      OnChanged: string -> unit
      OnDeleted: string -> unit }

    static member Default =
        { OnChanged = fun _ -> ()
          OnCreated = fun _ -> ()
          OnDeleted = fun _ -> () }

type DeRegisterWatch = unit -> unit

type GlossaryOperations =
    { Start: Glossary.StartGlossary -> Glossary.T
      Reload: Glossary.T -> unit
      Stop: Glossary.T -> unit }

// Create glossary with static dependencies
type CreateGlossaryManager = { GlossaryOps: GlossaryOperations }

// InitGlossary with dynamic dependencies that rely on the language server having already started
type InitGlossaryManager =
    { Log: Logger
      FileScanner: string array -> string seq
      RegisterWatchedFiles: OnWatchedFilesEventHandlers -> string array -> DeRegisterWatch
      DefaultGlossaryPathResolver: unit -> Async<Result<PathConfiguration, FileError>> }

type State =
    { Log: Logger
      FileScanner: string array -> string seq
      RegisterWatchedFiles: string array -> DeRegisterWatch
      Glossaries: Map<string, Glossary.T>
      GlossaryOps: GlossaryOperations
      DefaultGlossaryPathResolver: unit -> Async<Result<PathConfiguration, FileError>>
      DefaultGlossaryPath: string
      DefaultGlossary: Glossary.T option
      DeRegisterDefaultGlossaryFileWatcher: DeRegisterWatch option }

type Lookup =
    { Filter: Filter
      OpenFileUri: string
      Rc: AsyncReplyChannel<FindResult> }

type Message =
    | Init of InitGlossaryManager * OnWatchedFilesEventHandlers
    | ReloadDefaultGlossaryFile
    | WatchedFileCreated of string
    | WatchedFileChanged of string
    | Lookup of Lookup

type T = MailboxProcessor<Message>

let private GLOSSARY_FILE_GLOB = [| "**/*.glossary.yml"; "**/*.glossary.yaml" |]

module private Handlers =

    let create (createGlossary: CreateGlossaryManager) =

        let state =
            { FileScanner = fun _ -> []
              Log = Logger.Noop
              RegisterWatchedFiles = fun _ -> fun () -> ()
              Glossaries = Map []
              GlossaryOps = createGlossary.GlossaryOps
              DefaultGlossaryPathResolver = fun _ -> FileError.NotYetLoaded |> Error |> async.Return
              DefaultGlossary = None
              DefaultGlossaryPath = ""
              DeRegisterDefaultGlossaryFileWatcher = None }

        state

    let init (state: State) (initGlossary: InitGlossaryManager) (watchedFileshandlers: OnWatchedFilesEventHandlers) =
        let state =
            { state with
                FileScanner = initGlossary.FileScanner
                DefaultGlossaryPathResolver = initGlossary.DefaultGlossaryPathResolver
                RegisterWatchedFiles = initGlossary.RegisterWatchedFiles watchedFileshandlers
                Log = initGlossary.Log }

        GLOSSARY_FILE_GLOB |> state.RegisterWatchedFiles |> ignore

        let glossaryFiles = state.FileScanner GLOSSARY_FILE_GLOB

        let foundSubGlossaries =
            glossaryFiles
            |> Seq.map (fun p ->
                let glossary =
                    state.GlossaryOps.Start
                        { Path = { Path = p; IsDefault = false }
                          Log = state.Log }

                p, glossary)
            |> Map

        { state with
            Glossaries = foundSubGlossaries }


    let watchedFileChanged (state: State) path =
        let glossary = state.Glossaries[path]
        state.GlossaryOps.Reload glossary
        state

    let watchedFileCreated (state: State) path =
        let exists = state.Glossaries.ContainsKey(path)

        if exists then
            watchedFileChanged state path
        else
            let glossary =
                state.GlossaryOps.Start
                    { Path = { Path = path; IsDefault = false }
                      Log = state.Log }

            let newSubGlossaries = state.Glossaries.Add(path, glossary)

            { state with
                Glossaries = newSubGlossaries }

    let reloadDefaultGlossaryFile (state: State) =
        async {

            match state.DeRegisterDefaultGlossaryFileWatcher with
            | Some deregister -> deregister ()
            | _ -> ()

            let! pathOpt = state.DefaultGlossaryPathResolver()

            return
                pathOpt
                |> Result.map (fun path ->

                    let oldDefaultGlossaryPath = state.DefaultGlossaryPath
                    state.DefaultGlossary |> Option.map state.GlossaryOps.Stop |> ignore

                    let defaultGlossary = state.GlossaryOps.Start { Path = path; Log = state.Log }

                    { state with
                        DefaultGlossary = defaultGlossary |> Some
                        DefaultGlossaryPath = path.Path
                        Glossaries = state.Glossaries.Remove(oldDefaultGlossaryPath).Add(path.Path, defaultGlossary)
                        DeRegisterDefaultGlossaryFileWatcher = [| path.Path |] |> state.RegisterWatchedFiles |> Some })
                |> Result.mapError (fun fileError ->
                    let errorMessage = fileErrorMessage fileError
                    let msg = $"Error loading glossary file: {errorMessage}"
                    state.Log.error msg)
                |> Result.defaultValue state
        }

    let private pathMatch (openFilePath: string) (path: string) =
        path |> System.IO.Path.GetDirectoryName |> openFilePath.StartsWith

    let private normalizePath (p: string) =
        if System.String.IsNullOrEmpty(p) then
            System.IO.Path.DirectorySeparatorChar |> string
        else
            p

    let lookup (state: State) (lookup: Lookup) =
        async {
            let openFilePath =
                lookup.OpenFileUri |> System.IO.Path.GetDirectoryName |> normalizePath

            let results =
                state.Glossaries.Keys
                |> Seq.filter (fun p -> p <> state.DefaultGlossaryPath)
                |> Seq.filter (pathMatch openFilePath)
                |> Seq.map (fun p -> Glossary.lookup state.Glossaries[p] lookup.OpenFileUri lookup.Filter)
                |> AsyncSeq.ofSeqAsync
                |> AsyncSeq.toListSynchronously

            let defaultGlossaryResult =
                match state.DefaultGlossary with
                | Some glossary -> seq { Glossary.lookup glossary lookup.OpenFileUri lookup.Filter }
                | None -> seq { Seq.empty |> async.Return }
                |> AsyncSeq.ofSeqAsync
                |> AsyncSeq.toListSynchronously

            let combined =
                List.append results defaultGlossaryResult |> Seq.collect id |> List.ofSeq

            lookup.Rc.Reply combined

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

let create (createGlossary: CreateGlossaryManager) : T =
    MailboxProcessor.Start(fun inbox ->
        let rec loop (state: State) =
            async {
                let! (msg: Message) = inbox.Receive()

                let! newState = handleMessage state msg

                return! loop newState
            }

        loop <| Handlers.create createGlossary)

let init (glossary: T) (initGlossary: InitGlossaryManager) =
    Init(initGlossary, watchedFileHandlers glossary) |> glossary.Post

let reloadDefaultGlossaryFile (glossary: T) () =
    ReloadDefaultGlossaryFile |> glossary.Post

let lookup (glossary: T) (openFileUri: string) (filter: Filter) =
    glossary.PostAndAsyncReply(fun rc ->
        Lookup
            { Filter = filter
              OpenFileUri = openFileUri
              Rc = rc })
