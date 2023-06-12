module Contextive.Cloud.Api.Slack

open Microsoft.Extensions.Logging
open Giraffe

type SlackMessage = {
    Text: string
}

type SlackEvent = {
    Token: string
    Challenge: string 
    Type: string
    Event: SlackMessage
}

let post : HttpHandler = 
    fun next ctx -> task {
        let! event = ctx.BindJsonAsync<SlackEvent>()
        if event.Type = "url_verification" then 
            return! text event.Challenge next ctx
        else
            let logger = ctx.GetLogger("slack")
            logger.LogWarning $"Received text: {event.Event.Text}"
            return! text "" next ctx
    }

let routes:HttpHandler =
    choose [
        POST >=> route "/slack" >=> post
    ]