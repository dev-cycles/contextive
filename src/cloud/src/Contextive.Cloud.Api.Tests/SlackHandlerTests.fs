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
}""", RootName="Challenge">

type SlackMessage = JsonProvider<"""{
    "token": "one-long-verification-token",
    "team_id": "T123ABC456",
    "api_app_id": "A123ABC456",
    "event": {
        "type": "message",
        "channel": "C123ABC456",
        "user": "U123ABC456",
        "text": "Live long and prospect.",
        "ts": "1355517523.000005",
        "event_ts": "1355517523.000005",
        "channel_type": "channel"
    },
    "type": "event_callback",
    "authed_teams": [
        "T123ABC456"
    ],
    "event_id": "Ev123ABC456",
    "event_time": 1355517523
}""", RootName="Message">


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
            test <@ (response.StatusCode,response.Body) = (200, challengeValue) @>
        }

        testAsync "Can receive new message and new emoji notifications" {
            let lambdaFunction = LambdaEntryPoint()
            let msg = SlackMessage.GetSample()
            let! response = lambdaRequest lambdaFunction "POST" "/slack" (Some <| msg.JsonValue.ToString())
            test <@ (response.StatusCode, response.Body) = (200, "") @>
        }
    ]