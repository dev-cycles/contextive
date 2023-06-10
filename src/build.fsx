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

let getDefinitionsBucketName() =
    CreateProcess.fromRawCommandLine "awslocal" "s3api list-buckets"
      |> CreateProcess.redirectOutput
      |> CreateProcess.ensureExitCode
      |> Proc.run
      |> fun (a) -> a.Result.Output
      |> BucketResponse.Parse
      |> fun (b) -> b.Buckets
      |> Array.find (fun b -> b.Name.Contains("definitions"))
      |> fun (b) -> b.Name

// *** Define Targets ***
Target.create "Cloud-Api-Test" (fun _ ->

  Environment.setEnvironVar "DEFINITIONS_BUCKET_NAME" <| getDefinitionsBucketName()
  DotNet.exec (
        withWorkDir "cloud/src/Contextive.Cloud.Api.Tests"
      )
      "run" ""
  |> ensureSuccess
)

Target.create "Cloud-Deploy-Local" (fun _ ->
  CreateProcess.fromRawCommandLine "cdklocal" "deploy --context local="
  |> CreateProcess.withWorkingDirectory "cloud"
  |> CreateProcess.ensureExitCode
  |> Proc.run
  |> ignore
)

Target.create "Cloud-Deploy" (fun _ ->
  CreateProcess.fromRawCommandLine "cdk" "deploy"
  |> CreateProcess.withWorkingDirectory "cloud"
  |> CreateProcess.ensureExitCode
  |> Proc.run
  |> ignore
)

// open Fake.Core.TargetOperators

// // *** Define Dependencies ***
// "Clean"
//   ==> "Build"
//   ==> "Deploy"

// *** Start Build ***
Target.runOrDefault "Cloud-Api-Test"