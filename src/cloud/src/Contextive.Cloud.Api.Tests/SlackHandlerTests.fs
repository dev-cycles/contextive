module Contextive.Cloud.Api.Tests.SlackHandlerTests

open Expecto
open Swensen.Unquote
open Setup
open LambdaHelpers
open FSharp.Data

type SlackChallenge = JsonProvider<"""{
    "token": "token",
    "challenge": "challenge",
    "type": "url_verification"
}""", RootName="Challenge", PreferDictionaries=true>


[<Tests>]
let slackHandlerTests = 
    testList "Cloud.Api.SlackHandler" [

        testAsync "Can echo Slack Challenge" {
            let lambdaFunction = LambdaEntryPoint()
            let challengeValue = System.Guid.NewGuid().ToString()
            let challenge = SlackChallenge.Challenge(
                    token="token",
                    challenge=challengeValue,
                    ``type``="url_verification"
                )
            let! response = lambdaRequest lambdaFunction "POST" "/slack" (Some <| challenge.JsonValue.ToString())
            test <@ (response.StatusCode,challengeValue) = (200, response.Body) @>
        }
    ]