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
        printfn "%A" e
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
        Definitions: Definitions
        ConfigGetter: (unit -> Async<string option>)
        RegisterWatchedFile: (string option -> Unregisterer) option
        UnregisterLastWatchedFile: Unregisterer option
    }
    static member Initial() = { 
        WorkspaceFolder = None;
        Logger = fun _ -> ();
        Definitions = Definitions.Default;
        ConfigGetter = fun () -> async.Return None;
        RegisterWatchedFile = None
        UnregisterLastWatchedFile = None
    }

type LoadReply = string option

type Message =
    | Init of logger: (string -> unit) * configGetter: (unit -> Async<string option>) * registerWatchedFile: (string option -> Unregisterer) option
    | AddFolder of workspaceFolder: string option
    | Load
    | Find of filter: Filter * replyChannel: AsyncReplyChannel<Term seq>

let create() = MailboxProcessor.Start(fun inbox -> 
    let rec loop (state: State) = async {
        let! (msg: Message) = inbox.Receive()
        let! newState =
            match msg with
            | Init(logger, configGetter, registerWatchedFile) -> async.Return { 
                state with
                    Logger = logger;
                    ConfigGetter = configGetter;
                    RegisterWatchedFile = registerWatchedFile
                }
            | AddFolder(workspaceFolder) -> async.Return { state with WorkspaceFolder = workspaceFolder }
            | Load -> async {
                    let! path = state.ConfigGetter()
                    let absolutePath = getPath state.WorkspaceFolder path
                    let defs = loadFromPath absolutePath
                    let newState = match defs with 
                                    | Some d -> { state with Definitions = d }
                                    | _ -> { state with Definitions = Definitions.Default }
                    match absolutePath with
                    | Some ap -> state.Logger $"Loading contextly from {ap}"
                    | None -> ()
                    match state.UnregisterLastWatchedFile with
                    | Some unregister -> unregister()
                    | None -> ()                    
                    let unregisterer =
                        match state.RegisterWatchedFile with
                        | Some rwf -> rwf absolutePath |> Some
                        | None -> None
                    return { newState with UnregisterLastWatchedFile = unregisterer }
                }
            | Find(filter, replyChannel) ->
                let foundTerms = state.Definitions.Contexts |> Seq.collect(fun c -> c.Terms) |> Seq.filter filter
                replyChannel.Reply foundTerms
                async.Return state

        return! loop newState
    }
    loop <| State.Initial()
)

let init (definitionsManager:MailboxProcessor<Message>) logger configGetter registerWatchedFiles = 
    Init(logger, configGetter, registerWatchedFiles) |> definitionsManager.Post

let addFolder (definitionsManager:MailboxProcessor<Message>) workspaceFolder = 
    AddFolder(workspaceFolder) |> definitionsManager.Post

let loader (definitionsManager:MailboxProcessor<Message>) = fun () ->
    Load |> definitionsManager.Post

let find (definitionsManager:MailboxProcessor<Message>) (filter:Filter) =
    definitionsManager.PostAndAsyncReply <| fun rc -> Find(filter, rc)
