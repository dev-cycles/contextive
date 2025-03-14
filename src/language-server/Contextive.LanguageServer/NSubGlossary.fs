module Contextive.LanguageServer.NSubGlossary

open Contextive.Core.GlossaryFile
open Contextive.Core.File
open Logger

type StartSubGlossary = { Path: string; Log: Logger }

type Lookup =
    { Filter: Filter
      Rc: AsyncReplyChannel<FindResult> }

type Message =
    | Start of StartSubGlossary
    | Reload
    | Lookup of Lookup

type FileReaderFn = string -> Result<string, FileError>

type State =
    { FileReader: FileReaderFn
      GlossaryFile: GlossaryFile
      Log: Logger
      Path: string option }

    static member Initial =
        { FileReader = fun _ -> Error(NotYetLoaded)
          GlossaryFile = GlossaryFile.Default
          Log = Logger.Noop
          Path = None }

type T = MailboxProcessor<Message>

module Handlers =

    let reload (state: State) =
        async {
            return
                match state.Path with
                | None -> state
                | Some p ->
                    state.Log.info $"Loading contextive from {p}..."
                    let fileContents = state.FileReader p

                    fileContents
                    |> Result.bind deserialize
                    |> Result.map (fun r ->
                        state.Log.info "Successfully loaded."
                        { state with GlossaryFile = r })
                    |> Result.mapError (fun fileError ->
                        match fileError with
                        | DefaultFileNotFound ->
                            state.Log.info "No glossary file configured, and default file not found."
                        | _ ->
                            let errorMessage = fileErrorMessage fileError
                            let msg = $"Error loading glossary: {errorMessage}"
                            state.Log.info msg
                        //     state.OnErrorLoading msg
                        fileError)
                    |> Result.defaultValue state
        }

    let start state (startSubGlossary: StartSubGlossary) =
        reload
            { state with
                Path = Some startSubGlossary.Path
                Log = startSubGlossary.Log }



    let lookup (state: State) (lookup: Lookup) =
        async {
            lookup.Rc.Reply(state.GlossaryFile.Contexts)
            return state
        }

let private handleMessage (state: State) (msg: Message) =
    async {
        return!
            match msg with
            | Start startSubGlossary -> Handlers.start state startSubGlossary
            | Reload -> Handlers.reload state
            | Lookup p -> Handlers.lookup state p
    }


let start fileReader isDefault (startSubGlossary: StartSubGlossary) : T =
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
                   FileReader = fileReader isDefault
                   Path = None })

    Start startSubGlossary |> subGlossary.Post

    subGlossary

let reload (subGlossary: T) = Reload |> subGlossary.Post

let lookup (subGlossary: T) (filter: Filter) =
    subGlossary.PostAndAsyncReply(fun rc -> Lookup({ Filter = filter; Rc = rc }))
