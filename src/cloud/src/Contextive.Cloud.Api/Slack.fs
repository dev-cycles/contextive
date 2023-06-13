module Contextive.Cloud.Api.Slack

open Microsoft.Extensions.Logging
open Giraffe

module Receiving =
    type SlackMessage = {
        Type: string
        Channel: string
        Text: string
        User: string
        Channel_Type: string
        Ts: string
        Event_ts: string
    }

    type SlackEvent = {
        Token: string
        Challenge: string 
        Type: string
        Event: SlackMessage
    }

module Sending =
    open FSharp.Data
    type SlackMessage = JsonProvider<"""{"channel":"text","text":"text"}""">

let post : HttpHandler = 
    fun next ctx -> task {
        let! event = ctx.BindJsonAsync<Receiving.SlackEvent>()
        if event.Type = "url_verification" then 
            return! text event.Challenge next ctx
        else
            if event.Event.User <> "U02J05WTE75" && event.Event.Text <> null then
                let logger = ctx.GetLogger("slack")
                logger.LogWarning <| sprintf "Event text: %A" event.Event

                let msgText = $"Got: {event.Event}"
                let msg = Sending.SlackMessage.Root(channel=event.Event.Channel,text=msgText)
                let slackOAuth_Token = System.Environment.GetEnvironmentVariable("SLACK_OAUTH_TOKEN")
                let! res = msg.JsonValue.RequestAsync(
                    "https://slack.com/api/chat.postMessage",
                    headers = [ "Authorization", $"Bearer {slackOAuth_Token}" ]
                )
                logger.LogWarning <| sprintf "Post Response: %A" res
            
            return! text "" next ctx
    }

let routes:HttpHandler =
    choose [
        POST >=> route "/slack" >=> post
    ]