#r "nuget: Fun.Build, 1.0.5"
#r "nuget: FSharp.Data"
#r "nuget: FsToolkit.ErrorHandling"
#load "ci/common.fsx"

open Fun.Build
open FSharp.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Common

let hash = "8de71776e6e52a083e81be053c88eaf3aca82b87" // fail
let branch = ""

type GitHubRun =
    JsonProvider<Sample="""{
  "conclusion": "result",
  "databaseId": "typeof<int64>",
  "displayTitle": "commitmessage",
  "headSha": "hash",
  "status": "status",
  "url": "url"
}""", InferenceMode=InferenceMode.ValuesAndInlineSchemasOverrides>

type Fs<'a> = Printf.StringFormat<'a>

let ErrorMessages =
    {| NoRunFound =
        (Fs) "No run for hash '{%s}'. Ensure the commit is fully tested in a workflow run before attempting to release."
       FailedRun =
        (Fs)
            "Workflow run for hash '%s' concluded with '%s'. Only try and release commits with a passing workflow run. See %s for details."
       InProgressRun =
        (Fs)
            "Workflow run for hash '%s' is still '%s'. Wait for it to complete successfully before releasing. See %s for details." |}

let ghRunListCmd hash =
    sprintf
        """gh run list -w Contextive --json databaseId,conclusion,displayTitle,status,headSha,url -q 'map(select(.["headSha"] == "%s"))[]'"""
        hash

let tryParseGitHubRun json =
    try
        Ok(GitHubRun.Parse(json))
    with _ ->
        Error(sprintf ErrorMessages.NoRunFound hash)
    |> AsyncResult.ofResult

let checkConclusion: GitHubRun.Root -> _ =
    function
    | ghRun when ghRun.Status <> "completed" ->
        sprintf ErrorMessages.InProgressRun hash ghRun.Status ghRun.Url |> ghError
    | ghRun when ghRun.Conclusion = "success" -> Ok(ghRun)
    | ghRun -> sprintf ErrorMessages.FailedRun hash ghRun.Conclusion ghRun.Url |> ghError
    >> AsyncResult.ofResult

let getRunForHash (ctx: Internal.StageContext) =
    ctx.GetEnvVar(args.headSha.Name) |> ghRunListCmd |> runBashCaptureOutput <| ctx

let logSuccess (ghRun: GitHubRun.Root) =
    printfn "::notice ::See %s for the Workflow Run used to validate this release." ghRun.Url
    Ok() |> AsyncResult.ofResult

let checkReleaseStatus (ctx: Internal.StageContext) =
    getRunForHash ctx >>= tryParseGitHubRun >>= checkConclusion >>= logSuccess

pipeline "Contextive Release" {
    noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    stage "Check Build Status" {
        whenEnv { name args.headSha.Name }
        run checkReleaseStatus
    }

    stage "Install Dependencies" {
        workingDir "vscode/contextive"
        run "npm install"
    }

    stage "Install CI Dependencies" {
        workingDir "ci/semantic-release-markdown-to-html"
        run "npm install"
    }

    stage "Release" { run """npm exec --prefix "vscode/contextive" -- semantic-release""" }

    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
