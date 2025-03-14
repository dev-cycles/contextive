module Contextive.LanguageServer.NSubGlossary

open Contextive.Core.GlossaryFile
open Contextive.Core.File

type Lookup =
    { Filter: Filter
      Rc: AsyncReplyChannel<FindResult> }

type Message =
    | Start of string
    | Reload
    | Lookup of Lookup


type FileReaderFn = string -> Result<string, FileError>

type State =
    { FileReader: FileReaderFn
      GlossaryFile: GlossaryFile
      Path: string option }

    static member Initial =
        { FileReader = fun _ -> Error(NotYetLoaded)
          GlossaryFile = GlossaryFile.Default
          Path = None }

type T = MailboxProcessor<Message>

module Handlers =

    let reload (state: State) =
        async {
            return
                match state.Path with
                | None -> state
                | Some p ->
                    let fileContents = state.FileReader p

                    fileContents
                    |> Result.bind deserialize
                    |> Result.map (fun r -> { state with GlossaryFile = r })
                    |> Result.defaultValue state
        }

    let start state path =
        async {
            let _ = state.FileReader path
            return! reload { state with Path = Some path }
        }


    let lookup (state: State) (lookup: Lookup) =
        async {
            lookup.Rc.Reply(state.GlossaryFile.Contexts)
            return state
        }

let private handleMessage (state: State) (msg: Message) =
    async {
        return!
            match msg with
            | Start path -> Handlers.start state path
            | Reload -> Handlers.reload state
            | Lookup p -> Handlers.lookup state p
    }


let start fileReader path : T =
    let subGlossary =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (state: State) =
                async {
                    let! (msg: Message) = inbox.Receive()

                    let! newState = handleMessage state msg

                    return! loop newState
                }

            loop
            <| { State.Initial with
                   FileReader = fileReader
                   Path = None })

    Start(path) |> subGlossary.Post

    subGlossary

let reload (subGlossary: T) = Reload |> subGlossary.Post

let lookup (subGlossary: T) (filter: Filter) =
    subGlossary.PostAndAsyncReply(fun rc -> Lookup({ Filter = filter; Rc = rc }))
