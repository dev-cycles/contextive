module EventHandlerSetup
open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.SystemTextJson
open Contextive.Cloud.Api.SlackModels

[<assembly:LambdaSerializer(typeof<DefaultLambdaJsonSerializer>)>]
do ()

open Amazon.Lambda.CloudWatchEvents

let button text value =
    Sending.SlackMessage.Element(
        ``type``="button",
        text=Sending.SlackMessage.StringOrText(
            Sending.SlackMessage.Text(
                ``type``="plain_text",
                text=text
            )
        ),
        value=Some value
    )

let divider = 
    Sending.SlackMessage.Block(
        ``type``="divider",
        text=None,
        elements=[||]
    )

let header text = 
    Sending.SlackMessage.Block(
        ``type``="header",
        text=(Some <| Sending.SlackMessage.Text(
            ``type``="plain_text",
            text=text
        )),
        elements=[||]
    )

let termBlock termName description = 
    Sending.SlackMessage.Block(
        ``type``="context",
        text=None,
        elements=[|
            Sending.SlackMessage.Element(
                ``type``="mrkdwn",
                text=Sending.SlackMessage.StringOrText(sprintf "*%s*\n%s" termName description),
                value=None
            )
        |]
    )

let termActionsBlock =
    Sending.SlackMessage.Block(
        ``type``="actions",
        text=None,
        elements=[|
            button "Update Definition" "update_definition"
            button "See Usage Examples" "usage_examples"
            button "Usages in Other Contexts" "usages_in_other_contexts"
        |]
    )

let frequencyFooter = 
    Sending.SlackMessage.Block(
        ``type``="actions",
        text=None,
        elements=[|
            button "Change Reminder Frequency" "change_reminder_frequency"
        |]
    )

let shouldDrop = function
    | FSharp.Data.JsonValue.Array a when a.Length=0 -> true
    | FSharp.Data.JsonValue.Null -> true
    | _ -> false

let rec dropNullFields = function
  | FSharp.Data.JsonValue.Record flds ->
      flds 
      |> Array.choose (fun (k, v) -> 
        if shouldDrop v then None else
        Some(k, dropNullFields v) )
      |> FSharp.Data.JsonValue.Record
  | FSharp.Data.JsonValue.Array arr -> 
      arr |> Array.map dropNullFields |> FSharp.Data.JsonValue.Array
  | json -> json

let getBlocks termName = 
    [|
        header "Test Context Definition Reminders"
        divider
        termBlock termName "A description"
        termActionsBlock
        divider
        frequencyFooter
    |]

let getMessage channel text =
    (Sending.SlackMessage.Root(
        channel=channel,
        blocks=getBlocks text
    )).JsonValue |> dropNullFields

let FunctionHandlerAsync (evt : CloudWatchEvent<Receiving.SlackMessage>, context: ILambdaContext) = task {
    let event = evt.Detail
    let logger = context.Logger
    logger.LogWarning <| sprintf "Event text: %A" event

    let msg = getMessage event.Channel event.Text
    
    let slackOAuth_Token = System.Environment.GetEnvironmentVariable("SLACK_OAUTH_TOKEN")
    let! res = msg.RequestAsync(
        "https://slack.com/api/chat.postMessage",
        headers = [ "Authorization", $"Bearer {slackOAuth_Token}" ]
    )
    logger.LogWarning <| sprintf "Post Response: %A" res
}
