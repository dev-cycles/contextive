#r "nuget: Fun.Build, 1.1.7"

open Fun.Build

let args =
    {| dotnetRuntime =
        CmdArg.Create(
            "-r",
            "--runtime",
            "Dotnet Runtime for Publishing, see https://learn.microsoft.com/en-us/dotnet/core/rid-catalog#known-rids",
            isOptional = true
        )
       release = CmdArg.Create("-l", "--release", "Release version identified, e.g. v1.10.0")
       vscePlatform = CmdArg.Create("-p", "--vsce-platform", "VsCode platform Identifier, e.g. linux-x64, darwin-arm64")
       ci = EnvArg.Create("CI", "True if running in CI")
       dotnetVersion = EnvArg.Create("DOTNET_VERSION", "Version of the DotNet SDK", isOptional = true)
       os = EnvArg.Create("RUNNER_OS", "Operating System", isOptional = true)
       event = EnvArg.Create("GITHUB_EVENT_NAME", isOptional = true)
       ref = EnvArg.Create("GITHUB_REF", isOptional = true)
       repo = EnvArg.Create("GITHUB_REPOSITORY", isOptional = true)
       refName = EnvArg.Create("GITHUB_REF_NAME", isOptional = true)
       headSha = EnvArg.Create("GITHUB_SHA", isOptional = true)
       ghOutput = EnvArg.Create("GITHUB_OUTPUT", isOptional = true)
       runnerArch = EnvArg.Create("RUNNER_ARCH", isOptional = true) 
       vscePat = EnvArg.Create("VSCE_PAT", isOptional = true)
       ovsxPat = EnvArg.Create("OVSX_PAT", isOptional = true) |}

type Component = { Name: string; Path: string }

let TIMEOUT_EXIT_CODES = {| TIMED_OUT = 124; KILLED = 137 |}

let versionNumber (version: string) = version.Replace("v", "")

let bashCmd (cmd: string) =
    $"""bash -c "{cmd.Replace("\"", "\\\"")}" """

let runBash (cmd: string) (ctx: Internal.StageContext) = ctx.RunCommand(bashCmd cmd)

let runBashCaptureOutput (cmd: string) (ctx: Internal.StageContext) =
    ctx.RunCommandCaptureOutput(bashCmd cmd)

let ifTopLevelStage fn (ctx: Internal.StageContext) =
    match ctx.ParentContext with
    | ValueSome(Internal.StageParent.Pipeline _) -> fn ctx
    | _ -> ()

let ghError msg =
    printfn $"::error ::{msg}"
    Error(msg)

let appZipFileName app (ctx: Internal.StageContext) =
    $"{app.Name}-{ctx.GetCmdArg(args.dotnetRuntime)}-{versionNumber (ctx.GetCmdArg(args.release))}.zip"

let appZipPath app (ctx: Internal.StageContext) =
    System.IO.Path.GetFullPath(appZipFileName app ctx)

let zipCmd file zipPath =
    function
    | "Linux"
    | "" -> $"zip {zipPath} {file}"
    | _ -> $"7z a {zipPath} {file}"


let unzipCmd zipPath outputPath =
    function
    | "Linux"
    | "" -> $"unzip {zipPath} -d {outputPath}"
    | _ -> $"7z e {zipPath} -o{outputPath}"

let dotnetRestoreTools =
    stage $"Dotnet Restore Tools" { run "dotnet tool restore" }

let env (envArg: EnvArg) =
    System.Environment.GetEnvironmentVariable(envArg.Name)

let logEnvironment =
    stage "Log Environment" {
        run "dotnet --version"
        whenEnvVar args.event
        whenEnvVar args.os
        whenEnvVar args.ref
        whenEnvVar args.repo

        echo
            $"""
🎉 The job was automatically triggered by a {env args.event} event.
🐧 This job is now running on a {env args.os} server hosted by GitHub!
🔎 The name of your branch is {env args.ref} and your repository is {env args.repo}."""
    }


let core =
    { Name = "Contextive.Core"
      Path = "core/Contextive.Core" }

let languageServer =
    { Name = "Contextive.LanguageServer"
      Path = "language-server/Contextive.LanguageServer" }

let whenComponentInRelease (component': string) (folder: string) = whenStage $"Check for component {component'} in LAST_RELEASE_NOTES.md" {
    workingDir folder
    run (fun ctx -> 
        seq { component'; "language-server" }
            |> String.concat "|"
            |> sprintf "grep -E (%s) LAST_RELEASE_NOTES.md"
            |> ctx.RunCommand
    )
}
