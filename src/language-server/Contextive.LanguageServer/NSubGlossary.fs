module Contextive.LanguageServer.NSubGlossary

type Message =
    | Start of string
    | Reload

type FileReaderFn = string -> string

type State =
    { FileReader: FileReaderFn
      Path: string option }

type T = MailboxProcessor<Message>

module Handlers =
    let start state path =
        async {
            let _ = state.FileReader path
            return { state with Path = Some path }
        }

    let reload state =
        async {
            return
                match state.Path with
                | None -> state
                | Some p ->
                    let _ = state.FileReader p
                    state
        }

let private handleMessage (state: State) (msg: Message) =
    async {
        return!
            match msg with
            | Start path -> Handlers.start state path
            | Reload -> Handlers.reload state
    }


let create fileReader path : T =
    let subGlossary =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (state: State) =
                async {
                    let! (msg: Message) = inbox.Receive()

                    let! newState = handleMessage state msg

                    return! loop newState
                }

            loop <| { FileReader = fileReader; Path = None })

    Start(path) |> subGlossary.Post

    subGlossary



let reload (subGlossary: T) = Reload |> subGlossary.Post
