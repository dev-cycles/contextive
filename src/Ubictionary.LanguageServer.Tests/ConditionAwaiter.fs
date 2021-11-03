module Ubictionary.LanguageServer.Tests.ConditionAwaiter

type WaitForCondition<'T> = 
    {
        ReplyChannel: AsyncReplyChannel<'T>
        Condition: 'T -> bool
    }

type Message<'T> =
    | Received of 'T
    | WaitFor of WaitForCondition<'T>


let create<'T>() = MailboxProcessor.Start(fun inbox -> 
    let rec loop (conditions: WaitForCondition<'T> list) = async {
        let! (msg: Message<'T>) = inbox.Receive()
        let newState =
            match msg with
            | Received msg ->
                conditions |> Seq.iter (fun c -> if c.Condition msg then c.ReplyChannel.Reply(msg))
                conditions
            | WaitFor waitFor -> waitFor :: conditions
        return! loop newState
    }
    loop [])

let received (awaiter: MailboxProcessor<Message<'T>>) msg =
    awaiter.Post(Received(msg))

let waitFor (awaiter: MailboxProcessor<Message<'T>>) condition timeout =
    awaiter.PostAndTryAsyncReply((fun rc -> WaitFor({ReplyChannel=rc;Condition=condition})), timeout)