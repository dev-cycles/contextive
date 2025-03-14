module Contextive.LanguageServer.Glossary

open Contextive.Core.GlossaryFile

type OnWatchedFilesEventHandlers =
    { OnCreated: string -> unit
      OnChanged: string -> unit
      OnDeleted: string -> unit }

    static member Default =
        { OnChanged = fun _ -> ()
          OnCreated = fun _ -> ()
          OnDeleted = fun _ -> () }

type Logger = { info: string -> unit }

type DeRegisterWatch = unit -> unit

type SubGlossaryOperations =
    { Start: string -> NSubGlossary.T
      Reload: NSubGlossary.T -> unit }

// Create glossary with static dependencies
type CreateGlossary =
    { FileScanner: string -> string list
      SubGlossaryOps: SubGlossaryOperations }

// InitGlossary with dynamic dependencies that rely on the language server having already started
type InitGlossary =
    { Log: Logger
      RegisterWatchedFiles: OnWatchedFilesEventHandlers -> string option -> DeRegisterWatch
      DefaultSubGlossaryPathResolver: unit -> Async<string option> }

type State =
    { RegisterWatchedFiles: string option -> DeRegisterWatch
      SubGlossaries: Map<string, NSubGlossary.T>
      SubGlossaryOps: SubGlossaryOperations
      DefaultSubGlossaryPathResolver: unit -> Async<string option>
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
        let glossaryFiles = createGlossary.FileScanner GLOSSARY_FILE_GLOB

        glossaryFiles
        |> Seq.iter (fun p -> createGlossary.SubGlossaryOps.Start p |> ignore)

        let state =
            { RegisterWatchedFiles = fun _ -> fun () -> ()
              SubGlossaries = Map []
              SubGlossaryOps = createGlossary.SubGlossaryOps
              DefaultSubGlossaryPathResolver = fun _ -> async.Return None
              DefaultSubGlossary = None
              DeRegisterDefaultSubGlossaryFileWatcher = None }

        state

    let init (state: State) (initGlossary: InitGlossary) (watchedFileshandlers: OnWatchedFilesEventHandlers) =
        let state =
            { state with
                DefaultSubGlossaryPathResolver = initGlossary.DefaultSubGlossaryPathResolver
                RegisterWatchedFiles = initGlossary.RegisterWatchedFiles watchedFileshandlers }

        Some GLOSSARY_FILE_GLOB |> state.RegisterWatchedFiles |> ignore

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
            let subGlossary = state.SubGlossaryOps.Start path
            let newSubGlossaries = state.SubGlossaries.Add(path, subGlossary)

            { state with
                SubGlossaries = newSubGlossaries }

    let reloadDefaultGlossaryFile (state: State) =
        async {

            match state.DeRegisterDefaultSubGlossaryFileWatcher with
            | Some deregister -> deregister ()
            | _ -> ()

            let! pathOpt = state.DefaultSubGlossaryPathResolver()
            let path = pathOpt.Value

            let defaultSubGlossary = state.SubGlossaryOps.Start path

            return
                { state with
                    DefaultSubGlossary = defaultSubGlossary |> Some
                    SubGlossaries = state.SubGlossaries.Add(path, defaultSubGlossary)
                    DeRegisterDefaultSubGlossaryFileWatcher = Some path |> state.RegisterWatchedFiles |> Some }
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
