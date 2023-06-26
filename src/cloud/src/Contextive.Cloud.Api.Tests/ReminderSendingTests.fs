module Contextive.Cloud.Api.ReminderSendingTests

open Expecto
open Swensen.Unquote
open EventHandlerSetup
open System.Text.Json

[<Tests>]
let definitionsHandlerTests =
    testList
        "Cloud.Api.EventHandler"
        [

          ptestCase "Can construct a block message"
          <| fun _ ->

              let msg = getMessage "channel" "term"

              test <@ msg.ToString() = "" @> ]
