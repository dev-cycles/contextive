module Contextly.LanguageServer.Definitions

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.IO
open System.Collections.Concurrent

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
//type Loader = string option -> string option -> string option 
type Reloader = unit -> unit

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
        deserializer.Deserialize<Definitions>(yml) |> Some
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
        RegisterWatchedFiles: (string option -> unit)
    }
    static member Initial() = { 
        WorkspaceFolder = None;
        Logger = fun _ -> ();
        Definitions = Definitions.Default;
        ConfigGetter = fun () -> async.Return None;
        RegisterWatchedFiles = fun _ -> ()
    }

type LoadReply = string option

type Message =
    | Init of logger: (string -> unit) * configGetter: (unit -> Async<string option>) * registerWatchedFiles: (string option -> unit)
    | AddFolder of workspaceFolder: string option
    | Load
    | Find of filter: Filter * replyChannel: AsyncReplyChannel<Term seq>

let create() = MailboxProcessor.Start(fun inbox -> 
    let rec loop (state: State) = async {
        let! (msg: Message) = inbox.Receive()
        let! newState =
            match msg with
            | Init(logger, configGetter, registerWatchedFiles) -> async.Return { state with Logger = logger; ConfigGetter = configGetter; RegisterWatchedFiles = registerWatchedFiles}
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
                    state.RegisterWatchedFiles absolutePath
                    return newState
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
