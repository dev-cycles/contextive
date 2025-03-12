#r "nuget: FSharp.Data"
#r "nuget: FsToolkit.ErrorHandling"
#load "ci/common.fsx"

open Fun.Build
open Fun.Build.Github
open FSharp.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Common

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
    {| 
       NoRunFound = 
        (Fs) "No run found for hash '%s'. Please ensure a workflow has run before releasing."
       ParsingError =
        (Fs) "Unable to parse workflow run details for hash '%s'. Error message: %A."
       FailedRun =
        (Fs)
            "Workflow run for hash '%s' concluded with '%s'. Only try and release commits with a passing workflow run. See %s for details."
       InProgressRun =
        (Fs)
            "Workflow run for hash '%s' is still '%s'. Wait for it to complete successfully before releasing. See %s for details." |}

let ghRunListCmd workflowName hash  =
    sprintf
        """gh run list -w '%s' --json databaseId,conclusion,displayTitle,status,headSha,url -q '[.[]|select(.headSha=="%s")][0]'"""
        workflowName hash

let tryParseGitHubRun hash (json: string) =    
    try
        if json.Trim() = "" then
            Ok(None)
        else
            Ok(Some <| GitHubRun.Parse(json))
    with e ->
        Error(sprintf ErrorMessages.ParsingError hash (e))
    |> AsyncResult.ofResult

let checkConclusion hash =
    function
    | Some(ghRun: GitHubRun.Root) when ghRun.Status <> "completed" ->
        sprintf ErrorMessages.InProgressRun hash ghRun.Status ghRun.Url |> Error
    | Some(ghRun) when ghRun.Conclusion = "success" -> Ok(Some ghRun)
    | Some(ghRun) -> sprintf ErrorMessages.FailedRun hash ghRun.Conclusion ghRun.Url |> Error    
    | None -> Ok(None)
    >> AsyncResult.ofResult

let getRunForHash hash workflowName  ctx =
    hash |> ghRunListCmd workflowName |> runBashCaptureOutput <| ctx

let logSuccess (ghRun: GitHubRun.Root) =
    printfn "::notice ::See %s for the Workflow Run used to validate this release." ghRun.Url
    Ok() |> AsyncResult.ofResult

let checkReleaseStatus (ctx: Internal.StageContext) = async {
    let hash = ctx.GetEnvVar(args.headSha.Name)
    let! contextiveRun = getRunForHash hash "Contextive" ctx >>= tryParseGitHubRun hash >>= checkConclusion hash
    let! contextiveDocsRun = getRunForHash hash "Contextive Docs" ctx >>= (tryParseGitHubRun hash) >>= (checkConclusion hash)
       
    // If either ended in error, log an error.
    // If neither has a build at all, log the no run found error
    // If either passed and the other is either norun or also a pass, then go ahead.
    return!
        match contextiveRun, contextiveDocsRun with
        | Error(msg), _ | _, Error(msg) -> ghError msg
        | Ok(None), Ok(None) -> sprintf ErrorMessages.NoRunFound hash |> ghError
        | Ok(Some(ghRun)), Ok(_) | Ok(_), Ok(Some(ghRun)) -> Ok(ghRun)        
        |> Async.singleton
        |> AsyncResult.bind logSuccess}

pipeline "Contextive Release" {
    noPrefixForStep
    collapseGithubActionLogs

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
