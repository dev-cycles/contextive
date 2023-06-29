module Contextive.Cloud.Api.Slack

open Amazon.EventBridge
open Microsoft.Extensions.Logging
open Giraffe
open System.Text.Json
open AwsHelpers
open SlackModels

let eventBusName () =
    System.Environment.GetEnvironmentVariable("EVENT_BUS_NAME")

let post: HttpHandler =
    fun next ctx ->
        task {
            let! event = ctx.BindJsonAsync<Receiving.SlackEvent>()

            if event.Type = "url_verification" then
                return! text event.Challenge next ctx
            else
                let logger = ctx.GetLogger("slack")

                if event.Event.User <> "U02J05WTE75" && event.Event.Text <> null then
                    use eventBridgeClient =
                        new AmazonEventBridgeClient(AmazonEventBridgeConfig() |> forEnvironment)

                    logger.LogInformation($"Sending event, text is '{event.Event.Text}'")

                    try
                        let! res =
                            eventBridgeClient.PutEventsAsync(
                                Model.PutEventsRequest(
                                    Entries =
                                        ResizeArray<Model.PutEventsRequestEntry>
                                            [ Model.PutEventsRequestEntry(
                                                  EventBusName = eventBusName (),
                                                  Source = "contextive.api",
                                                  DetailType = ($"{event.Type}.{event.Event.Type}"),
                                                  Detail = JsonSerializer.Serialize(event.Event)
                                              ) ]
                                ),
                                System.Threading.CancellationToken.None
                            )

                        logger.LogInformation($"Sent {res}.")
                    with ex ->
                        logger.LogError(ex.ToString())
                        raise ex
                else
                    logger.LogInformation(
                        $"NOT sending event as channel is {event.Event.Channel} or text is '{event.Event.Text}'"
                    )

                return! text "" next ctx
        }

let routes: HttpHandler = choose [ POST >=> route "/slack" >=> post ]
