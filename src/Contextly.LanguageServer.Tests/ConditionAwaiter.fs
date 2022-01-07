module Contextly.LanguageServer.Tests.ConditionAwaiter

type WaitForCondition<'T> = 
    {
        ReplyChannel: AsyncReplyChannel<'T>
        Condition: 'T -> bool
    }

type private WaitState<'T> =
    {
        Conditions: WaitForCondition<'T> list
        Messages: 'T list
    }
    static member Initial = { Conditions = List.empty<WaitForCondition<'T>>; Messages = [] }

type Message<'T> =
    | Received of 'T
    | WaitFor of WaitForCondition<'T>
    | Clear


let create<'T>() = MailboxProcessor.Start(fun inbox -> 
    let rec loop (state: WaitState<'T>) = async {
        let! (msg: Message<'T>) = inbox.Receive()
        let newState =
            match msg with
            | Received msg ->
                state.Conditions |> Seq.iter (fun w -> if w.Condition msg then w.ReplyChannel.Reply(msg))
                { state with Messages = msg :: state.Messages }
            | WaitFor waitFor ->
                state.Messages |> Seq.iter (fun m -> if waitFor.Condition m then waitFor.ReplyChannel.Reply(m))
                { state with Conditions = waitFor :: state.Conditions }
            | Clear ->
                { state with Messages = [] }
        return! loop newState
    }
    loop WaitState<'T>.Initial
)

let received (awaiter: MailboxProcessor<Message<'T>>) msg =
    awaiter.Post(Received(msg))

let waitFor (awaiter: MailboxProcessor<Message<'T>>) condition timeout =
    awaiter.PostAndTryAsyncReply((fun rc -> WaitFor({ReplyChannel=rc;Condition=condition})), timeout)

let waitForAny (awaiter: MailboxProcessor<Message<'T>>) timeout =
    waitFor awaiter (fun _ -> true) timeout

let clear (awaiter : MailboxProcessor<Message<'T>>) timeout =
    awaiter.Post(Clear)