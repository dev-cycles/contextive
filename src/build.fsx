#if FAKE
#r "paket:
nuget FSharp.Core 6
nuget FSharp.Data
nuget Fake.DotNet.Cli
nuget Fake.Core.Target //"
#endif
#load ".fake/build.fsx/intellisense.fsx"
// include Fake modules, see Fake modules section

open Fake.Core
open Fake.DotNet
open FSharp.Data

let inline withWorkDir wd = DotNet.Options.withWorkingDirectory wd
let ensureSuccess (p:ProcessResult) =
  match p with
    | p when p.ExitCode = 0 -> ()
    | p -> failwith (String.concat (System.Environment.NewLine) p.Errors) 

type BucketResponse = JsonProvider<""" { "Buckets": [{"Name": "SampleName"}] } """>

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
  cdk' "cdklocal" cmd <| Some "--context local="

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

let getDefinitionsBucketName() =
    awsLocal "s3api list-buckets" (
        fun output ->
        BucketResponse.Parse output
        |> fun b -> b.Buckets
        |> Array.find (fun b -> b.Name.Contains("definitions"))
        |> fun b -> b.Name
    )

// *** Define Targets ***
Target.create "Cloud-Api-Test" (fun _ ->
  setLocal
  Environment.setEnvironVar "DEFINITIONS_BUCKET_NAME" <| getDefinitionsBucketName()
  DotNet.exec (
        withWorkDir "cloud/src/Contextive.Cloud.Api.Tests"
      )
      "run" ""
  |> ensureSuccess
)

Target.create "Cloud-Api-Publish" (fun _ ->
  setLocal
  DotNet.exec (
        withWorkDir "cloud/src/Contextive.Cloud.Api"
      )
      "publish" "--runtime linux-x64 --no-self-contained"
  |> ensureSuccess
)

Target.create "Cdk-Bootstrap-Local" (fun _ ->
  cdkLocal "bootstrap"
)

Target.create "Cloud-Deploy-Local" (fun _ ->
  cdkLocal "deploy"
)

Target.create "Cloud-Deploy" (fun _ ->
  cdk "deploy"
)

open Fake.Core.TargetOperators

// // *** Define Dependencies ***
// "Clean"
//   ==> "Build"
//   ==> "Deploy"

"Cloud-Api-Publish"
  ==> "Cloud-Deploy"

// *** Start Build ***
Target.runOrDefault "Cloud-Api-Test"