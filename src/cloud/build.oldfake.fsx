#r "paket:
nuget FSharp.Core 7
nuget FSharp.Data
nuget Fake.DotNet.Cli
nuget Fake.Core.Target //"

//#load ".fake/build.fsx/intellisense.fsx"
// include Fake modules, see Fake modules section

open Fake.Core
open Fake.DotNet
open FSharp.Data


#if !FAKE
let execContext = Fake.Core.Context.FakeExecutionContext.Create false "build.fsx" []
Fake.Core.Context.setExecutionContext (Fake.Core.Context.RuntimeContext.Fake execContext)
#endif

//Context.setExecutionContext (Context.Fake(FakeExecutionContext.Create true "" [||]))

let inline withWorkDir wd = DotNet.Options.withWorkingDirectory wd

let ensureSuccess (p: ProcessResult) =
    match p with
    | p when p.ExitCode = 0 -> ()
    | p -> failwith (String.concat (System.Environment.NewLine) p.Errors)

type BucketResponse = JsonProvider<""" { "Buckets": [{"Name": "SampleName"}] } """>
type EventBusResponse = JsonProvider<""" { "EventBuses": [{"Name": "SampleName"}] } """>

let setLocal = Environment.setEnvironVar "AWS_PROFILE" "local"

let cdk' cdkType cmd args =
    let argString = Option.defaultValue "" args

    CreateProcess.fromRawCommandLine cdkType $"{cmd} {argString}"
    |> CreateProcess.withWorkingDirectory "cloud"
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let cdkLocal cmd =
    setLocal
    cdk' "cdklocal" cmd <| Some "--context local= --require-approval never"

let cdk cmd =
    Environment.setEnvironVar "AWS_PROFILE" "dev-cycles-sandbox-chris"
    cdk' "cdk" cmd None

let awsLocal cmd filter =
    CreateProcess.fromRawCommandLine "awslocal" cmd
    |> CreateProcess.redirectOutput
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> fun a -> a.Result.Output
    |> filter

let getDefinitionsBucketName () =
    awsLocal "s3api list-buckets" (fun output ->
        BucketResponse.Parse output
        |> fun b -> b.Buckets
        |> Array.find (fun b -> b.Name.Contains("definitions"))
        |> fun b -> b.Name)

let getEventBusName () =
    awsLocal "events list-event-buses" (fun output ->
        EventBusResponse.Parse output
        |> fun b -> b.EventBuses
        |> Array.find (fun b -> b.Name.Contains("Slack"))
        |> fun b -> b.Name)

// *** Define Targets ***
Target.create "Cloud-Api-Test" (fun _ ->
    setLocal

    Environment.setEnvironVar "DEFINITIONS_BUCKET_NAME"
    <| getDefinitionsBucketName ()

    Environment.setEnvironVar "EVENT_BUS_NAME" <| getEventBusName ()
    Environment.setEnvironVar "IS_LOCAL" "True"

    DotNet.exec (withWorkDir "cloud/src/Contextive.Cloud.Api.Tests") "run" ""
    |> ensureSuccess)

Target.create "Cloud-Api-Publish" (fun _ ->
    setLocal

    DotNet.exec (withWorkDir "cloud/src/Contextive.Cloud.Api") "publish" "--runtime linux-x64 --no-self-contained"
    |> ensureSuccess)

Target.create "Cdk-Bootstrap-Local" (fun _ -> cdkLocal "bootstrap")

Target.create "Cloud-Deploy-Local" (fun _ -> cdkLocal "deploy")

Target.create "Cloud-Deploy" (fun _ -> cdk "deploy")

Target.create "Cloud-Synth" (fun _ -> cdk "synth")

open Fake.Core.TargetOperators

"Cloud-Api-Publish" ==> "Cloud-Deploy"

"Cloud-Api-Publish" ==> "Cdk-Bootstrap-Local"

"Cloud-Api-Publish" ==> "Cloud-Deploy-Local"

"Cdk-Bootstrap-Local" ==> "Cloud-Deploy-Local"

"Cloud-Deploy-Local" ==> "Cloud-Api-Test"

// *** Start Build ***
Target.runOrDefault "Cloud-Api-Test"
// Target.runOrDefault "Cloud-Deploy"
// Target.runOrDefault "Cdk-Bootstrap-Local"
