module Contextly.LanguageServer.Definitions

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.IO

[<CLIMutable>]
type Term =
    {
        Name: string
        Definition: string option
        Examples: ResizeArray<string>
    }
    static member Default = {Name = ""; Definition = None; Examples = null}

[<CLIMutable>]
type Context =
    {
        Terms: ResizeArray<Term>
    }

[<CLIMutable>]
type Definitions =
    {
        Contexts: ResizeArray<Context>
    }
    static member Default = { Contexts = new ResizeArray<Context>() }

type Filter = Term -> bool
type Finder = Filter -> Async<Term seq>
type Reloader = unit -> unit
type Unregisterer = unit -> unit

let private tryReadFile path =
    if File.Exists(path) then
      File.ReadAllText(path) |> Some
    else
      None

let private deserialize (yml:string) =
    try
        let deserializer = 
            (new DeserializerBuilder())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
        let definitions = deserializer.Deserialize<Definitions>(yml)
        match definitions |> box with
        | null -> None
        | _ -> Some definitions
    with
    | e -> 
        None

let private loadContextly path =
    match tryReadFile path with
    | None -> None
    | Some(ymlText) -> deserialize ymlText

let private getPath workspaceFolder (path: string option) =
    match path with
    | None -> None
    | Some p -> 
        if Path.IsPathRooted(p) then
            path
        else match workspaceFolder with
             | Some wsf -> Path.Combine(wsf, p) |> Some
             | None -> None

let private loadFromPath (absolutePath:string option) =
    match absolutePath with
    | Some ap ->
        loadContextly ap
    | None -> None

type private State =
    {
        WorkspaceFolder: string option
        Logger: (string -> unit)
        DefinitionsFilePath: string option
        Definitions: Definitions
        ConfigGetter: (unit -> Async<string option>)
        RegisterWatchedFile: (string option -> Unregisterer) option
        UnregisterLastWatchedFile: Unregisterer option
        OnErrorLoading: (string -> unit)
    }
    static member Initial() = { 
        WorkspaceFolder = None;
        Logger = fun _ -> ();
        DefinitionsFilePath = None;
        Definitions = Definitions.Default;
        ConfigGetter = fun () -> async.Return None;
        RegisterWatchedFile = None
        UnregisterLastWatchedFile = None
        OnErrorLoading = fun _ -> ();
    }

type LoadReply = string option

type Message =
    | Init of
        logger: (string -> unit) *
        configGetter: (unit -> Async<string option>) *
        registerWatchedFile: (string option -> Unregisterer) option *
        onErrorLoading: (string -> unit)
    | AddFolder of workspaceFolder: string option
    | Load
    | Find of filter: Filter * replyChannel: AsyncReplyChannel<Term seq>

let private handleMessage (state: State) (msg: Message) = async {
    return! match msg with
            | Init(logger, configGetter, registerWatchedFile, onErrorLoading) -> async.Return { 
                state with
                    Logger = fun msg -> 
                                async {
                                    logger msg
                                } |> Async.Start
                    ConfigGetter = configGetter;
                    RegisterWatchedFile = registerWatchedFile
                    OnErrorLoading = onErrorLoading;
                }
            | AddFolder(workspaceFolder) -> async.Return { state with WorkspaceFolder = workspaceFolder }
            | Load -> async {
                    let! path = state.ConfigGetter()
                    let absolutePath = getPath state.WorkspaceFolder path

                    match absolutePath with
                    | Some ap -> state.Logger $"Loading contextly from {ap}..."
                    | None -> ()

                    state.Logger $"Load from path {absolutePath}..."
                    let defs = loadFromPath absolutePath
                    state.Logger $"Returned defs {defs}..."
                    let newState = 
                        match defs with 
                        | Some d -> { state with Definitions = d }
                        | _ -> { state with Definitions = Definitions.Default }
                    state.Logger $"newState {newState}..."
                    
                    match absolutePath, defs with
                    | Some _, Some _ -> state.Logger "Succesfully loaded."
                    | Some ap, None -> state.Logger $"Error loading definitions.  Please check syntax in {ap}"
                    | _, _ -> ()

                    let returnState = 
                        match state.DefinitionsFilePath, absolutePath with
                        | Some existingPath, Some newPath when existingPath = newPath -> newState
                        | _, _ -> 
                            match state.UnregisterLastWatchedFile with
                            | Some unregister -> 
                                state.Logger $"Unwatching {state.DefinitionsFilePath}."
                                unregister()
                            | None -> ()

                            let unregisterer =
                                match state.RegisterWatchedFile with
                                | Some rwf -> 
                                    state.Logger $"Watching {absolutePath}."
                                    rwf absolutePath |> Some
                                | None -> None

                            { newState with UnregisterLastWatchedFile = unregisterer; DefinitionsFilePath = absolutePath }
                    state.Logger $"About to return state {returnState}"
                    return returnState
                    
                }
            | Find(filter, replyChannel) ->
                let foundTerms = state.Definitions.Contexts |> Seq.collect(fun c -> c.Terms) |> Seq.filter filter
                replyChannel.Reply foundTerms
                async.Return state
}

let create() = MailboxProcessor.Start(fun inbox -> 
    let rec loop (state: State) = async {
        let! (msg: Message) = inbox.Receive()
        let! newState =
            try
                handleMessage state msg
            with
                | e -> 
                    printfn "%A" e
                    state.Logger <| e.ToString()
                    async.Return state
        return! loop newState
    }
    loop <| State.Initial()
)

let init (definitionsManager:MailboxProcessor<Message>) logger configGetter registerWatchedFiles onErrorLoading = 
    Init(logger, configGetter, registerWatchedFiles, onErrorLoading) |> definitionsManager.Post

let addFolder (definitionsManager:MailboxProcessor<Message>) workspaceFolder = 
    AddFolder(workspaceFolder) |> definitionsManager.Post

let loader (definitionsManager:MailboxProcessor<Message>) = fun () ->
    Load |> definitionsManager.Post

let find (definitionsManager:MailboxProcessor<Message>) (filter:Filter) =
    let msgBuilder = fun rc -> Find(filter, rc)
    definitionsManager.PostAndAsyncReply(msgBuilder, 1000)
