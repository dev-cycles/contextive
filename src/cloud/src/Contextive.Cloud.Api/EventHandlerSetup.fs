module EventHandlerSetup

open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.SystemTextJson
open Contextive.Cloud.Api.SlackModels
open Contextive.Cloud.Api.DefinitionsRepository
open Contextive.Core.Definitions
open Contextive.Core.File

[<assembly: LambdaSerializer(typeof<DefaultLambdaJsonSerializer>)>]
do ()

open Amazon.Lambda.CloudWatchEvents

let button text value =
    Sending.SlackMessage.Element(
        ``type`` = "button",
        text = Sending.SlackMessage.StringOrText(Sending.SlackMessage.Text(``type`` = "plain_text", text = text)),
        value = Some value
    )

let divider =
    Sending.SlackMessage.Block(``type`` = "divider", text = None, elements = [||])

let header text =
    Sending.SlackMessage.Block(
        ``type`` = "section",
        text =
            (Some
             <| Sending.SlackMessage.Text(``type`` = "mrkdwn", text = sprintf "*%s*" text)),
        elements = [||]
    )

let termDefinitionBlock termName description =
    Sending.SlackMessage.Block(
        ``type`` = "context",
        text = None,
        elements =
            [| Sending.SlackMessage.Element(
                   ``type`` = "mrkdwn",
                   text = Sending.SlackMessage.StringOrText(sprintf "*%s*\n%s" termName (defaultArg description "")),
                   value = None
               ) |]
    )

let termActionsBlock =
    Sending.SlackMessage.Block(
        ``type`` = "actions",
        text = None,
        elements =
            [| button "Update Definition" "update_definition"
               button "See Usage Examples" "usage_examples"
               button "Usages in Other Contexts" "usages_in_other_contexts" |]
    )

let termBlock (term: Term) =
    [| termDefinitionBlock term.Name term.Definition; termActionsBlock; divider |]

let frequencyFooter =
    [| Sending.SlackMessage.Block(
           ``type`` = "context",
           text = None,
           elements =
               [| Sending.SlackMessage.Element(
                      ``type`` = "mrkdwn",
                      text =
                          Sending.SlackMessage.StringOrText(
                              "Reminders for these terms won't be sent for the next 2 weeks."
                          ),
                      value = None
                  ) |]
       )
       Sending.SlackMessage.Block(
           ``type`` = "actions",
           text = None,
           elements = [| button "Change Reminder Frequency" "change_reminder_frequency" |]
       ) |]

let shouldDrop =
    function
    | FSharp.Data.JsonValue.Array a when a.Length = 0 -> true
    | FSharp.Data.JsonValue.Null -> true
    | _ -> false

let rec dropNullFields =
    function
    | FSharp.Data.JsonValue.Record flds ->
        flds
        |> Array.choose (fun (k, v) -> if shouldDrop v then None else Some(k, dropNullFields v))
        |> FSharp.Data.JsonValue.Record
    | FSharp.Data.JsonValue.Array arr -> arr |> Array.map dropNullFields |> FSharp.Data.JsonValue.Array
    | json -> json

let getBlocks context =
    let terms = context.Terms |> Seq.map termBlock |> Seq.collect id

    [| header $"{context.Name} Reminders"
       divider
       yield! terms
       yield! frequencyFooter |]

let getMessage (channel: string) (thread_ts: string) definitions =
    let context = definitions.Contexts[0]

    (Sending.SlackMessage.Root(channel = channel, threadTs = thread_ts, blocks = getBlocks context))
        .JsonValue
    |> dropNullFields

let sendMessageToChannel (logger: ILambdaLogger) (msg: FSharp.Data.JsonValue) =
    task {
        let slackOAuth_Token =
            System.Environment.GetEnvironmentVariable("SLACK_OAUTH_TOKEN")

        let! res =
            msg.RequestAsync(
                "https://slack.com/api/chat.postMessage",
                headers = [ "Authorization", $"Bearer {slackOAuth_Token}" ]
            )

        logger.LogWarning <| sprintf "Post Response: %A" res
    }

let getCandidateTokens (msg: string) =
    msg.Split(" ")
    |> Seq.map (fun s -> (s, Some s |> Contextive.Core.CandidateTerms.candidateTermsFromToken))

let filterContext tokens context =
    let terms =
        context.Terms
        |> Seq.filter (Contextive.Core.CandidateTerms.termFilterForCandidateTerms tokens)

    { context with
        Terms = ResizeArray<Term>(terms) }

let lookForMatchingTerms (msg: string) (definitions: Definitions) =
    let tokens = getCandidateTokens msg
    let contexts = Seq.map (filterContext tokens) definitions.Contexts

    if (Seq.head contexts).Terms.Count = 0 then
        Error(ParsingError("No terms found."))
    else
        Ok
            { definitions with
                Contexts = ResizeArray<Context>(contexts) }

let formatMessage (channel: string) (thread_ts: string) (definitions: Definitions) =
    getMessage channel thread_ts definitions |> Ok

let FunctionHandlerAsync (evt: CloudWatchEvent<Receiving.SlackMessage>, context: ILambdaContext) =
    task {
        let event = evt.Detail
        let logger = context.Logger
        logger.LogWarning <| sprintf "Event text: %A" event
        let! res = getDefinitions "demo"

        let msg =
            res
            |> Result.bind deserialize
            |> Result.bind (lookForMatchingTerms event.Text)
            |> Result.bind (formatMessage event.Channel event.Ts)

        do!
            match msg with
            | Ok(m) -> sendMessageToChannel logger m
            | Error(e) -> task { logger.LogError((e.ToString())) }
    }
