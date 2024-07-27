#r "nuget: Fun.Build, 1.0.5"
#r "nuget: FSharp.Data"
#r "nuget: FsToolkit.ErrorHandling"
#load "../ci/common.fsx"

open Fun.Build
open FSharp.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Common

type BucketResponse = JsonProvider<""" { "Buckets": [{"Name": "SampleName"}] } """>
type EventBusResponse = JsonProvider<""" { "EventBuses": [{"Name": "SampleName"}] } """>
type ApiGatewayResponse = JsonProvider<"""{ "Items": [{ "ApiEndpoint": "https://host.name" }] }""">


let awsLocal cmd ctx =
    $"awslocal {cmd}" |> runBashCaptureOutput <| ctx

let aws cmd ctx =
    $"aws {cmd}" |> runBashCaptureOutput <| ctx

let getDefinitionsBucketName (ctx) =
    awsLocal "s3api list-buckets" ctx
    >>= (fun output ->
        BucketResponse.Parse output
        |> fun b -> b.Buckets
        |> Array.find (fun b -> b.Name.Contains("definitions"))
        |> fun b -> b.Name
        |> Ok
        |> AsyncResult.ofResult)

let getEventBusName (ctx) =
    awsLocal "events list-event-buses" ctx
    >>= (fun output ->
        EventBusResponse.Parse output
        |> fun b -> b.EventBuses
        |> Array.find (fun b -> b.Name.Contains("Slack"))
        |> fun b -> b.Name
        |> Ok
        |> AsyncResult.ofResult)

let getEndpoint (ctx) =
    aws "apigatewayv2 get-apis" ctx
    >>= (fun output ->
        ApiGatewayResponse.Parse output
        |> fun i -> i.Items |> Seq.head
        |> fun a -> a.ApiEndpoint
        |> Ok
        |> AsyncResult.ofResult)

let cloudApiTest =
    stage "Cloud Api Test" {
        envVars [ "AWS_PROFILE", "local"; "IS_LOCAL", "True" ]

        workingDir "cloud/src/Contextive.Cloud.Api.Tests"

        run (fun ctx ->
            asyncResult {
                let! ebn = getEventBusName ctx
                let! dbn = getDefinitionsBucketName ctx
                return! runBash $"DEFINITIONS_BUCKET_NAME={dbn} EVENT_BUS_NAME={ebn} dotnet run --no-spinner" ctx
            })
    }

let cdkCmd' cdkType cmd = $"{cdkType} {cmd}"

let cdkLocalCmd cmd =
    cdkCmd' "cdklocal" $"{cmd} --context local= --require-approval never"

let cdkCmd cmd = cdkCmd' "cdk" cmd

let cdkDeployLocal =
    stage "CDK Deploy Local" {
        envVars [ "AWS_PROFILE", "local"; "IS_LOCAL", "True" ]
        workingDir "cloud"

        run (cdkLocalCmd "deploy")
    }

let cdkBootStrapLocal =
    stage "CDK BootStrap Local" {
        envVars [ "AWS_PROFILE", "local"; "IS_LOCAL", "True" ]
        workingDir "cloud"

        run (cdkLocalCmd "bootstrap")
    }

let cloudApiPublish =
    stage "Cloud Api Publish" {
        envVars [ "AWS_PROFILE", "local"; "IS_LOCAL", "True" ]
        workingDir "cloud/src/Contextive.Cloud.Api"
        run "dotnet publish --runtime linux-x64 --no-self-contained"
    }

let prodRegion = [ "AWS_REGION", "eu-west-3" ]

let cloudDeploy =
    stage "Cloud Deploy" {
        workingDir "cloud"
        envVars prodRegion
        run (cdkCmd "deploy")
    }

let cloudE2eTest =
    stage "Cloud E2E Test" {
        workingDir "cloud/src/Contextive.Cloud.Tests.E2e"
        envVars prodRegion

        run (fun ctx ->
            asyncResult {
                let! definitionsUrl = getEndpoint ctx
                return! runBash $"CONTEXTIVE_ENDPOINT_URL={definitionsUrl} dotnet run --no-spinner" ctx
            })
    }

pipeline "Contextive Cloud Local Test" {
    noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    cloudApiPublish

    cdkBootStrapLocal

    cdkDeployLocal

    cloudApiTest

    runIfOnlySpecified false
}

pipeline "Contextive Cloud Deploy" {
    noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    cloudApiPublish

    cloudDeploy

    cloudE2eTest

    runIfOnlySpecified true
}

tryPrintPipelineCommandHelp ()
