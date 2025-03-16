module Contextive.LanguageServer.Glossary

open Contextive.Core.GlossaryFile
open Contextive.LanguageServer.Logger
open Contextive.Core.File
open System.Collections.Generic
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

type SubGlossaryOperations =
    { Start: SubGlossary.StartSubGlossary -> SubGlossary.T
      Reload: SubGlossary.T -> unit }

// Create glossary with static dependencies
type CreateGlossary =
    { SubGlossaryOps: SubGlossaryOperations }

// InitGlossary with dynamic dependencies that rely on the language server having already started
type InitGlossary =
    { Log: Logger
      FileScanner: string -> string list
      RegisterWatchedFiles: OnWatchedFilesEventHandlers -> string option -> DeRegisterWatch
      DefaultSubGlossaryPathResolver: unit -> Async<Result<PathConfiguration, FileError>> }

type State =
    { Log: Logger
      FileScanner: string -> string list
      RegisterWatchedFiles: string option -> DeRegisterWatch
      SubGlossaries: Map<string, SubGlossary.T>
      SubGlossaryOps: SubGlossaryOperations
      DefaultSubGlossaryPathResolver: unit -> Async<Result<PathConfiguration, FileError>>
      DefaultSubGlossaryPath: string
      DefaultSubGlossary: SubGlossary.T option
      DeRegisterDefaultSubGlossaryFileWatcher: DeRegisterWatch option }

type Lookup =
    { Filter: Filter
      OpenFileUri: string
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
            { FileScanner = fun _ -> []
              Log = Logger.Noop
              RegisterWatchedFiles = fun _ -> fun () -> ()
              SubGlossaries = Map []
              SubGlossaryOps = createGlossary.SubGlossaryOps
              DefaultSubGlossaryPathResolver = fun _ -> FileError.NotYetLoaded |> Error |> async.Return
              DefaultSubGlossary = None
              DefaultSubGlossaryPath = ""
              DeRegisterDefaultSubGlossaryFileWatcher = None }

        state

    let init (state: State) (initGlossary: InitGlossary) (watchedFileshandlers: OnWatchedFilesEventHandlers) =
        let state =
            { state with
                FileScanner = initGlossary.FileScanner
                DefaultSubGlossaryPathResolver = initGlossary.DefaultSubGlossaryPathResolver
                RegisterWatchedFiles = initGlossary.RegisterWatchedFiles watchedFileshandlers
                Log = initGlossary.Log }

        Some GLOSSARY_FILE_GLOB |> state.RegisterWatchedFiles |> ignore

        let glossaryFiles = state.FileScanner GLOSSARY_FILE_GLOB

        let foundSubGlossaries =
            glossaryFiles
            |> Seq.map (fun p ->
                let subGlossary =
                    state.SubGlossaryOps.Start
                        { Path = { Path = p; IsDefault = false }
                          Log = state.Log }

                p, subGlossary)
            |> Map

        { state with
            SubGlossaries = foundSubGlossaries }


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
                        DefaultSubGlossaryPath = path.Path
                        SubGlossaries = state.SubGlossaries.Add(path.Path, defaultSubGlossary)
                        DeRegisterDefaultSubGlossaryFileWatcher = Some path.Path |> state.RegisterWatchedFiles |> Some })
                |> Result.mapError (fun fileError ->
                    let errorMessage = fileErrorMessage fileError
                    let msg = $"Error loading glossary: {errorMessage}"
                    state.Log.error msg)
                |> Result.defaultValue state
        }

    let private isDefault (subGlossary: SubGlossary.T) (defaultSubGlossary: SubGlossary.T option) =
        defaultSubGlossary.IsSome && subGlossary = defaultSubGlossary.Value

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
                state.SubGlossaries.Keys
                |> Seq.filter (fun p -> p <> state.DefaultSubGlossaryPath)
                |> Seq.filter (pathMatch openFilePath)
                |> Seq.map (fun p -> SubGlossary.lookup state.SubGlossaries[p] lookup.OpenFileUri lookup.Filter)
                |> AsyncSeq.ofSeqAsync
                |> AsyncSeq.toListSynchronously

            let defaultSubGlossaryResult =
                match state.DefaultSubGlossary with
                | Some subGlossary -> seq { SubGlossary.lookup subGlossary lookup.OpenFileUri lookup.Filter }
                | None -> seq { Seq.empty |> async.Return }
                |> AsyncSeq.ofSeqAsync
                |> AsyncSeq.toListSynchronously

            let combined =
                List.append results defaultSubGlossaryResult |> Seq.collect id |> List.ofSeq

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

let lookup (glossary: T) (openFileUri: string) (filter: Filter) =
    glossary.PostAndAsyncReply(fun rc ->
        Lookup
            { Filter = filter
              OpenFileUri = openFileUri
              Rc = rc })
