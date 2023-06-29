namespace Contextive.Cloud

open System
open Amazon.CDK
open Amazon.CDK.AWS.S3
open Amazon.CDK.AWS.Events
open Amazon.CDK.AWS.Lambda

type ContextiveCloudStack(scope, id, props) as this =
    inherit Stack(scope, id, props)

    let definitions = Bucket(this, "Definitions", BucketProps())

    do
        CfnOutput(this, "DefinitionsBucketName", CfnOutputProps(Value = definitions.BucketName))
        |> ignore

    let eventBus = EventBus(this, "SlackEvents", EventBusProps())

    let eventHandlerFunctionProps =
        LambdaFunctions.props this "Event Handler" "EventHandlerSetup" definitions eventBus

    let eventHandlerFunction = Function(this, "EventHandler", eventHandlerFunctionProps)

    do
        Rule(
            this,
            "eventHandler",
            RuleProps(
                EventBus = eventBus,
                EventPattern = EventPattern(DetailType = [| "event_callback.message" |]),
                Targets = [| Targets.LambdaFunction(eventHandlerFunction) |]
            )
        )
        |> ignore

    let apiFunctionProps =
        LambdaFunctions.props this "Api" "Setup+LambdaEntryPoint" definitions eventBus

    let apiFunction = Function(this, "Api", apiFunctionProps)

    do eventBus.GrantPutEventsTo(apiFunction) |> ignore
    do definitions.GrantReadWrite(apiFunction) |> ignore
    do definitions.GrantRead(eventHandlerFunction) |> ignore

    do
        match Context.isLocal this with
        | false -> HttpApi.setup this apiFunction
        | _ ->
            printfn "Local: Not adding http API"
            ()
