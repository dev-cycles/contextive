#load "../ci/common.fsx"
#load "../ci/dotnet.fsx"

open Fun.Build
open Fun.Build.Github
open Common
open Dotnet

let cli =
    { Name = "Contextive.Cli"
      Path = "cli/Contextive.Cli" }

let cliLabel (ctx: Internal.StageContext) =
    $"Contextive CLI {ctx.GetCmdArg(args.release)} ({ctx.GetCmdArg(args.dotnetRuntime)})"

pipeline cli.Name {
    description "Build & Test"
    // noPrefixForStep
    collapseGithubActionLogs

    logEnvironment

    stage "Install Tools" {
        whenEnv { name args.event.Name }
        dotnetRestoreTools
    }

    stage "Install Dependencies" {
        workingDir "cli"
        run "dotnet paket restore"
    }

    stage "Build & Test" { dotnetTest cli }

    stage "Build Release" {
        dotnetPublish cli

        zipAndUploadAsset cli
    }


    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
