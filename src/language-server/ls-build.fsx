#load "../ci/common.fsx"
#load "../ci/dotnet.fsx"
#load "app.fsx"
#load "../core/app.fsx"

open Fun.Build
open Fun.Build.Github
open Common
open Dotnet
open LanguageServer.App
open Core.App

let languageServerLabel (ctx: Internal.StageContext) =
    $"Contextive Language Server {ctx.GetCmdArg(args.release)} ({ctx.GetCmdArg(args.dotnetRuntime)})"

let checkRelease app =
    stage "Check Release" {
        workingDir $"{app.Path}/publish"

        // Windows doesn't support timeout command
        whenAny {
            platformLinux
            platformOSX
        }

        // gh runners are only x64
        // when https://github.com/actions/runner-images/issues/5631 is done (support arm64)
        // we might be able to run this on linux-arm64 and osx-arm64 as well.
        whenCmd {
            name args.dotnetRuntime.Name.Names.Head
            acceptValues [ "linux-x64"; "osx-x64" ]
        }

        // MacOs doesn't have the timeout command installed by default
        stage "Install Timeout on MacOs" {
            whenOSX

            run "brew install coreutils"
        }

        // We're trying to catch scenarios where the language server exits immediately on it's own
        // so these scenarios where timeout terminates or kills mean the language server is
        // running successfull.
        // The 'KILL' is required for running locally in docker.
        acceptExitCodes [ TIMEOUT_EXIT_CODES.TIMED_OUT; TIMEOUT_EXIT_CODES.KILLED ]

        run (bashCmd $"timeout -v -k 1 2 ./{app.Name}")
    }

pipeline languageServer.Name {
    description "Build & Test"
    // noPrefixForStep
    collapseGithubActionLogs

    logEnvironment

    stage "Install Tools" {
        whenEnv { name args.event.Name }
        dotnetRestoreTools
    }

    stage "Install Dependencies" {
        workingDir "language-server"
        run "dotnet paket restore"
    }

    stage "Build & Test" {
        dotnetTest core
        dotnetTest languageServer
    }

    stage "Build Release" {
        dotnetPublish languageServer

        checkRelease languageServer

        zipAndUploadAsset languageServer
    }


    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
