module Contextive.LanguageServer.Glossary

type OnWatchedFilesEventHandlers =
    { OnCreated: string -> unit
      OnChanged: string -> unit
      OnDeleted: string -> unit }

    static member Default =
        { OnChanged = fun _ -> ()
          OnCreated = fun _ -> ()
          OnDeleted = fun _ -> () }

type Logger = { info: string -> unit }

type SubGlossaryOperations = { Create: string -> NSubGlossary.T }

type CreateGlossary =
    { FileScanner: string -> string list
      Log: Logger
      RegisterWatchedFiles: string -> OnWatchedFilesEventHandlers -> unit
      SubGlossaryOps: SubGlossaryOperations }

type State =
    { RegisterWatchedFiles: string -> OnWatchedFilesEventHandlers -> unit
      DefaultGlossaryFile: string option }

type Message = SetDefaultGlossaryFile of string

type T = MailboxProcessor<Message>

[<Literal>]
let private GLOSSARY_FILE_GLOB = "**/*.contextive.yml"

module private Handlers =
    let create (createGlossary: CreateGlossary) =
        let glossaryFiles = createGlossary.FileScanner GLOSSARY_FILE_GLOB

        glossaryFiles
        |> Seq.iter (fun p ->
            createGlossary.Log.info $"Found definitions file at '{p}'..."
            createGlossary.SubGlossaryOps.Create p |> ignore)

        createGlossary.RegisterWatchedFiles GLOSSARY_FILE_GLOB OnWatchedFilesEventHandlers.Default

        { RegisterWatchedFiles = createGlossary.RegisterWatchedFiles
          DefaultGlossaryFile = None }

    let setDefaultGlossaryFile (state: State) path =
        state.RegisterWatchedFiles path OnWatchedFilesEventHandlers.Default

        { state with
            DefaultGlossaryFile = Some path }

let private handleMessage (state: State) (msg: Message) =
    async {
        return
            match msg with
            | SetDefaultGlossaryFile path -> Handlers.setDefaultGlossaryFile state path
    }

let create (createGlossary: CreateGlossary) : T =
    MailboxProcessor.Start(fun inbox ->
        let rec loop (state: State) =
            async {
                let! (msg: Message) = inbox.Receive()

                let! newState = handleMessage state msg

                return! loop newState
            }

        loop <| Handlers.create createGlossary)


let setDefaultGlossaryFile (glossary: T) path =
    SetDefaultGlossaryFile(path) |> glossary.Post
