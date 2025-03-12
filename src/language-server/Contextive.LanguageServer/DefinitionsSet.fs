module Contextive.LanguageServer.DefinitionsSet

// type State = { initialDefinitions: Definitions.T }
// type Message = { idx: int }

// type T = MailboxProcessor<Message>

// let private handleMessage state msg = async.Return state


// let create () = 
//     MailboxProcessor.Start(fun inbox ->
//         let rec loop (state: State) =
//             async {
//                 let! (msg: Message) = inbox.Receive()

//                 let! newState =
//                     try
//                         handleMessage state msg
//                     with e ->
//                         printfn "%A" e
//                         async.Return state

//                 return! loop newState
//             }

//         loop <| { initialDefinitions = Definitions.create () }
//     )