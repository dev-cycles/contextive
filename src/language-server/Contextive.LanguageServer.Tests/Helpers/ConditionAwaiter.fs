module Contextive.LanguageServer.Tests.Helpers.ConditionAwaiter

type WaitForCondition<'T> =
    { ReplyChannel: AsyncReplyChannel<'T>
      Condition: 'T -> bool }

type private WaitState<'T> =
    { Conditions: WaitForCondition<'T> list
      Messages: 'T list }

    static member Initial =
        { Conditions = List.empty<WaitForCondition<'T>>
          Messages = [] }

type Message<'T> =
    | Received of 'T
    | WaitFor of WaitForCondition<'T>
    | Clear

type Awaiter<'T> = MailboxProcessor<Message<'T>>

[<Literal>]
let private DEFAULT_TIMEOUT_MS = 5000

let create<'T> () =
    let awaiter =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (state: WaitState<'T>) =
                async {
                    let! (msg: Message<'T>) = inbox.Receive()

                    let newState =
                        match msg with
                        | Received msg ->
                            state.Conditions
                            |> Seq.iter (fun w ->
                                if w.Condition msg then
                                    w.ReplyChannel.Reply(msg))

                            { state with
                                Messages = msg :: state.Messages }
                        | WaitFor waitFor ->
                            state.Messages
                            |> Seq.iter (fun m ->
                                if waitFor.Condition m then
                                    waitFor.ReplyChannel.Reply(m))

                            { state with
                                Conditions = waitFor :: state.Conditions }
                        | Clear -> { state with Messages = [] }

                    return! loop newState
                }

            loop WaitState<'T>.Initial)

    awaiter.Error.Add(fun e -> printfn "%A" e)
    awaiter

let received (awaiter: Awaiter<'T>) msg = awaiter.Post(Received(msg))

/// Waits for a message that passes the condition to become true up to the nominated timeout.
let waitForTimeout timeoutMs (awaiter: Awaiter<'T>) condition =
    awaiter.PostAndTryAsyncReply(
        (fun rc ->
            WaitFor(
                { ReplyChannel = rc
                  Condition = condition }
            )),
        timeoutMs
    )

/// Waits for a message that passes the condition to become true up to the default timeout of 5s.
/// If you want a custom timeout, use waitForTimeout.
let waitFor (awaiter: Awaiter<'T>) =
    waitForTimeout DEFAULT_TIMEOUT_MS awaiter

/// Waits for any message up to the nominated timeout.
let waitForAnyTimeout timeoutMs (awaiter: Awaiter<'T>) =
    waitForTimeout timeoutMs awaiter (fun _ -> true)

/// Waits for any message up to the default timeout of 5s.
/// If you want a custom timeout, use waitForAnyTimeout.
let waitForAny awaiter =
    waitForAnyTimeout DEFAULT_TIMEOUT_MS awaiter

let clear (awaiter: Awaiter<'T>) = awaiter.Post(Clear)
