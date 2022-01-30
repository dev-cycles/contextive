module Contextive.LanguageServer.Definitions

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open DotNet.Globbing
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
        Name: string
        DomainVisionStatement: string
        Paths: ResizeArray<string>
        Terms: ResizeArray<Term>
    }

[<CLIMutable>]
type Definitions =
    {
        Contexts: ResizeArray<Context>
    }
    static member Default = { Contexts = new ResizeArray<Context>() }

type Filter = Term -> bool
type Finder = string -> Filter -> Async<Term seq>
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
        | null -> Error "Definitions file is empty."
        | _ -> Ok definitions
    with
    | :? YamlDotNet.Core.YamlException as e -> 
        let msg = if e.InnerException = null then e.Message else e.InnerException.Message
        Error $"Error parsing definitions file:  Object starting line {e.Start.Line}, column {e.Start.Column} - {msg}"

let private loadContextive path =
    match tryReadFile path with
    | None -> None
    | Some(ymlText) -> deserialize ymlText |> Some

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
        loadContextive ap
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

type InitPayload = {
    Logger: (string -> unit);
    ConfigGetter: (unit -> Async<string option>);
    RegisterWatchedFile: (string option -> Unregisterer) option;
    OnErrorLoading: (string -> unit);
}

type AddFolderPayload = {
    WorkspaceFolder: string option
}

type FindReplyChannel = AsyncReplyChannel<Term seq>

type FindPayload = {
    OpenFileUri: string;
    Filter: Filter;
    ReplyChannel: FindReplyChannel
}
 
type Message =
    | Init of InitPayload
    | AddFolder of AddFolderPayload
    | Load
    | Find of FindPayload

module private Handle =
    let init state (initMsg:InitPayload) : State =
        { 
            state with
                Logger = initMsg.Logger
                ConfigGetter = initMsg.ConfigGetter
                RegisterWatchedFile = initMsg.RegisterWatchedFile
                OnErrorLoading = initMsg.OnErrorLoading
        }

    let addFolder state (addFolderMsg:AddFolderPayload) : State = 
        { state with WorkspaceFolder = addFolderMsg.WorkspaceFolder }

    let updateFileWatchers (state:State) (absolutePath:string option)=
        match state.DefinitionsFilePath, absolutePath with
        | Some existingPath, Some newPath when existingPath = newPath -> state
        | _, _ -> 
            match state.UnregisterLastWatchedFile with
            | Some unregister -> unregister()
            | None -> ()

            let unregisterer =
                match state.RegisterWatchedFile with
                | Some rwf -> rwf absolutePath |> Some
                | None -> None

            { state with UnregisterLastWatchedFile = unregisterer; DefinitionsFilePath = absolutePath }

    let load (state:State) : Async<State> = async {
            let! path = state.ConfigGetter()
            let absolutePath = getPath state.WorkspaceFolder path

            absolutePath |> Option.iter (fun ap -> state.Logger $"Loading contextive from {ap}...") 

            let defs = loadFromPath absolutePath

            let newState = match defs with
                            | Some loadResult ->
                                match loadResult with
                                | Ok defs -> 
                                    state.Logger "Succesfully loaded."
                                    { state with Definitions = defs }
                                | Error msg -> 
                                    let msg = $"Error loading definitions: {msg}"
                                    Serilog.Log.Logger.Error msg
                                    state.Logger msg
                                    state.OnErrorLoading msg
                                    { state with Definitions = Definitions.Default }
                            | None -> state

            return updateFileWatchers newState absolutePath
        }

    let matchPreciseGlob (openFileUri:string) pathGlob =
        Glob.Parse(pathGlob)
            .IsMatch(openFileUri)

    let matchGlob (openFileUri:string) pathGlob =
        let formats: Printf.StringFormat<string->string> list =
            [
                "%s"
                "**%s"
                "**%s*"
                "**%s**"
            ]
        formats
        |> List.map (fun s -> sprintf s pathGlob)
        |> List.exists (matchPreciseGlob openFileUri)

    let matchGlobs openFileUri (context:Context) =
        let matchOpenFileUri = matchGlob openFileUri
        match context.Paths with
        | null -> true
        | _ -> context.Paths |> Seq.exists matchOpenFileUri
    
    let extractTerms t = t.Terms
    
    let find (state: State) (findMsg: FindPayload) : State =
        let matchOpenFileUri = matchGlobs findMsg.OpenFileUri
        let foundTerms =
            state.Definitions.Contexts
            |> Seq.filter matchOpenFileUri
            |> Seq.collect extractTerms
            |> Seq.filter findMsg.Filter
        findMsg.ReplyChannel.Reply foundTerms
        state

let private handleMessage (state: State) (msg: Message) = async {
    return!
        match msg with
        | Init(initMsg) -> Handle.init state initMsg |> async.Return
        | AddFolder(workspaceFolderMsg) -> Handle.addFolder state workspaceFolderMsg |> async.Return 
        | Load -> Handle.load state
        | Find(findMsg) -> Handle.find state findMsg |> async.Return
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

let init (definitionsManager:MailboxProcessor<Message>) logger configGetter registerWatchedFile onErrorLoading = 
    Init({Logger=logger; ConfigGetter=configGetter; RegisterWatchedFile=registerWatchedFile; OnErrorLoading=onErrorLoading})
    |> definitionsManager.Post

let addFolder (definitionsManager:MailboxProcessor<Message>) workspaceFolder = 
    AddFolder({WorkspaceFolder=workspaceFolder}) |> definitionsManager.Post

let loader (definitionsManager:MailboxProcessor<Message>) = fun () ->
    Load |> definitionsManager.Post

let find (definitionsManager:MailboxProcessor<Message>) (openFileUri:string) (filter:Filter) =
    let msgBuilder = fun rc -> Find({OpenFileUri=openFileUri;Filter=filter;ReplyChannel=rc})
    definitionsManager.PostAndAsyncReply(msgBuilder, 1000)