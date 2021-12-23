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
type Finder = (Term -> bool) -> Term seq
type Loader = string option -> string option -> string option 
type Reloader = unit -> string option

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

let private load (absolutePath:string option) (logger:string -> unit) =
    match absolutePath with
    | Some ap ->
        logger $"Loading contextly from {ap}"
        loadContextly ap
    | None -> None

module Manager =

    type State =
        {
            WorkspaceFolder: string option
            Logger: (string -> unit)
            Definitions: Definitions
        }
        static member Initial() = { 
            WorkspaceFolder = None;
            Logger = fun _ -> ();
            Definitions = Definitions.Default;
        }

    type Message =
        | Init of logger: (string -> unit)
        | AddFolder of workspaceFolder: string option
        | Load of path: string option * replyChannel: AsyncReplyChannel<string option>
        | Find of filter: Filter * replyChannel: AsyncReplyChannel<Term seq>

    let create() = MailboxProcessor.Start(fun inbox -> 
        let rec loop (state: State) = async {
            let! (msg: Message) = inbox.Receive()
            let newState =
                match msg with
                | Init(logger) -> { state with Logger = logger}
                | AddFolder(workspaceFolder) -> { state with WorkspaceFolder = workspaceFolder }
                | Load(path, replyChannel) ->
                    let absolutePath = getPath state.WorkspaceFolder path
                    let defs = load absolutePath state.Logger
                    let newState = match defs with 
                                    | Some d -> { state with Definitions = d }
                                    | _ -> { state with Definitions = Definitions.Default }
                    replyChannel.Reply absolutePath
                    newState
                | Find(filter, replyChannel) ->
                    let foundTerms = state.Definitions.Contexts |> Seq.collect(fun c -> c.Terms) |> Seq.filter filter
                    replyChannel.Reply foundTerms
                    state

            return! loop newState
        }
        loop <| State.Initial()
    )

    let init (definitionsManager:MailboxProcessor<Message>) logger = 
        Init(logger) |> definitionsManager.Post

    let addFolder (definitionsManager:MailboxProcessor<Message>) workspaceFolder = 
        AddFolder(workspaceFolder) |> definitionsManager.Post
        
    let load (definitionsManager:MailboxProcessor<Message>) path =
        definitionsManager.PostAndReply <| fun rc -> Load(path, rc)

    let find (definitionsManager:MailboxProcessor<Message>) (filter:Filter) =
        definitionsManager.PostAndReply <| fun rc -> Find(filter, rc)
