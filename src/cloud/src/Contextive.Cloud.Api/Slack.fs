module Contextive.Cloud.Api.Slack

open Giraffe
open FSharp.Data
open Microsoft.Extensions.Logging

type SlackChallenge = {
    Token: string
    Challenge: string
    Type: string
}

let post : HttpHandler = 
    fun next ctx -> task {
        let! challenge = ctx.BindJsonAsync<SlackChallenge>()
        return! text challenge.Challenge next ctx
    }

let routes:HttpHandler =
    choose [
        POST >=> route "/slack" >=> post
    ]