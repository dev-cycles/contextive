#r "nuget: Fun.Build, 1.0.5"
#load "../ci/common.fsx"

open Fun.Build

open Common

let core =
    { Name = "Contextive.Core"
      Path = "core/Contextive.Core" }

let languageServer =
    { Name = "Contextive.LanguageServer"
      Path = "language-server/Contextive.LanguageServer" }

let dotnetTest app =
    stage $"Test {app.Name}" {
        workingDir $"{app.Path}.Tests"
        whenEnvVar args.dotnetVersion
        whenEnvVar args.os

        run
            $"""dotnet test --logger "trx;LogFileName=TestResults.{app.Name}-{env args.dotnetVersion}-{env args.os}.xml" -- Expecto.fail-on-focused-tests=true Expecto.no-spinner=true Expecto.summary=true"""
    }

let dotnetPublish app =
    stage $"Publish {app.Name}" {
        workingDir app.Path
        whenCmdArg args.dotnetRuntime

        run (fun ctx ->
            let runTimeFlag =
                match ctx.GetCmdArg(args.dotnetRuntime) with
                | r when not <| System.String.IsNullOrEmpty(r) -> $"-r {r}"
                | _ -> ""

            $"""dotnet publish -c RELEASE {runTimeFlag} -o publish""")
    }

let appZipFileName app (ctx: Internal.StageContext) =
    System.IO.Path.GetFullPath($"{app.Name}-{ctx.GetCmdArg(args.dotnetRuntime)}-{ctx.GetCmdArg(args.release)}.zip")

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

        acceptExitCodes [ 124 ]

        run (bashCmd $"timeout -v 2 ./{app.Name}")
    }

let zipCmd =
    function
    | "Linux"
    | "" -> "zip"
    | _ -> "7z a"

let publishedBinaryName app =
    function
    | "Windows" -> $"{app.Name}.exe"
    | _ -> app.Name

let zipAndUploadAsset app =
    stage $"Zip And Upload {app.Name} Release Asset" {
        stage "Zip" {
            workingDir $"{app.Path}/publish"

            run (fun ctx ->
                let path = appZipFileName app ctx
                let os = ctx.GetEnvVar(args.os.Name)
                let zip = zipCmd os
                let binaryName = publishedBinaryName app os

                $"{zip} {path} {binaryName}")

            stage "Upload" {
                workingDir app.Path

                whenCmdArg args.release

                run (fun ctx -> $"gh release upload {ctx.GetCmdArg(args.release)} {appZipFileName app ctx}")
            }
        }
    }

pipeline languageServer.Name {
    description "Build & Test"
    // noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    logEnvironment

    stage "Install Tools" {
        whenEnv { name args.event.Name }
        installTool "paket"
    }

    stage "Install Dependencies" {
        workingDir "language-server"
        run "paket restore"
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
