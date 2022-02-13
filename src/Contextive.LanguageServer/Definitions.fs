module Contextive.LanguageServer.Definitions

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open DotNet.Globbing
open System.Linq
open FileLoader

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
    static member Default = { Name=""; DomainVisionStatement=""; Paths=new ResizeArray<string>(); Terms=new ResizeArray<Term>()}
    member this.WithTerms (terms:Term seq) = { this with Terms = terms.ToList() }

[<CLIMutable>]
type Definitions =
    {
        Contexts: ResizeArray<Context>
    }
    static member Default = { Contexts = new ResizeArray<Context>() }

type Filter = Term -> bool
type FindResult = Context seq
type Finder = string -> Filter -> Async<FindResult>

type FileLoader = unit -> Async<Result<File, string>>

type Reloader = unit -> unit
type Unregisterer = unit -> unit

module FindResult =
    let allTerms (contexts:FindResult) : Term seq = contexts |> Seq.collect(fun c -> c.Terms)

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

type private State =
    {
        WorkspaceFolder: string option
        Logger: (string -> unit)
        DefinitionsFilePath: string option
        Definitions: Definitions
        FileLoader: FileLoader
        RegisterWatchedFile: (string option -> Unregisterer) option
        UnregisterLastWatchedFile: Unregisterer option
        OnErrorLoading: (string -> unit)
    }
    static member Initial() = { 
        WorkspaceFolder = None;
        Logger = fun _ -> ();
        DefinitionsFilePath = None;
        FileLoader = fun _ -> async.Return <| Error("Path not yet defined.");
        Definitions = Definitions.Default;
        RegisterWatchedFile = None
        UnregisterLastWatchedFile = None
        OnErrorLoading = fun _ -> ();
    }

type LoadReply = string option

type InitPayload = {
    Logger: (string -> unit);
    FileLoader: FileLoader;
    RegisterWatchedFile: (string option -> Unregisterer) option;
    OnErrorLoading: (string -> unit);
}

type AddFolderPayload = {
    WorkspaceFolder: string option
}

type FindReplyChannel = AsyncReplyChannel<FindResult>

type FindPayload = {
    OpenFileUri: string;
    Filter: Filter;
    ReplyChannel: FindReplyChannel
}
 
type Message =
    | Init of InitPayload
    | Load
    | Find of FindPayload

let private loadFile (state: State) (file: Result<File, string>) =
    match file with
    | Error(e) -> Error(e)
    | Ok({AbsolutePath=ap;Contents=contents}) -> 
        state.Logger $"Loading contextive from {ap}..."
        match contents with
        | Ok(c) -> deserialize c
        | Error(e) -> Error(e)

module private Handle =
    let init state (initMsg:InitPayload) : State =
        { 
            state with
                Logger = initMsg.Logger
                RegisterWatchedFile = initMsg.RegisterWatchedFile
                OnErrorLoading = initMsg.OnErrorLoading
                FileLoader = initMsg.FileLoader
        }

    let updateFileWatchers (state:State) (file:Result<File, string>)=
        match state.DefinitionsFilePath, file with
        | Some existingPath, Ok({AbsolutePath=newPath}) when existingPath = newPath -> state
        | _, Ok({AbsolutePath=newPath}) -> 
            match state.UnregisterLastWatchedFile with
            | Some unregister -> unregister()
            | None -> ()
            let path = Some newPath
            let unregisterer =
                match state.RegisterWatchedFile with
                | Some rwf -> rwf path |> Some
                | None -> None

            { state with UnregisterLastWatchedFile = unregisterer; DefinitionsFilePath = path }
        | _, _ -> state

    let load (state:State) : Async<State> = async {
            let! file = state.FileLoader()
            
            let defs = loadFile state file

            let newState = match defs with
                            | Ok defs -> 
                                state.Logger "Succesfully loaded."
                                { state with Definitions = defs }
                            | Error msg -> 
                                let msg = $"Error loading definitions: {msg}"
                                Serilog.Log.Logger.Error msg
                                state.Logger msg
                                state.OnErrorLoading msg
                                { state with Definitions = Definitions.Default }

            return updateFileWatchers newState file
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
        let foundContexts =
            state.Definitions.Contexts
            |> Seq.filter matchOpenFileUri
            |> Seq.map (fun c -> c.WithTerms(c.Terms |> Seq.filter findMsg.Filter))
        findMsg.ReplyChannel.Reply foundContexts
        state

let private handleMessage (state: State) (msg: Message) = async {
    return!
        match msg with
        | Init(initMsg) -> Handle.init state initMsg |> async.Return
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

let init (definitionsManager:MailboxProcessor<Message>) logger fileLoader registerWatchedFile onErrorLoading = 
    Init({Logger=logger; FileLoader=fileLoader; RegisterWatchedFile=registerWatchedFile; OnErrorLoading=onErrorLoading})
    |> definitionsManager.Post

let loader (definitionsManager:MailboxProcessor<Message>) = fun () ->
    Load |> definitionsManager.Post

let find (definitionsManager:MailboxProcessor<Message>) (openFileUri:string) (filter:Filter) =
    let msgBuilder = fun rc -> Find({OpenFileUri=openFileUri;Filter=filter;ReplyChannel=rc})
    definitionsManager.PostAndAsyncReply(msgBuilder, 1000)