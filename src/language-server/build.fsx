#r "nuget: Fun.Build, 1.0.5"
#load "../ci/common.fsx"

open Fun.Build

open Common

let languageServerLabel (ctx: Internal.StageContext) =
    $"Contextive Language Server {ctx.GetCmdArg(args.release)} ({ctx.GetCmdArg(args.vscePlatform)})"


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

let publishedBinaryName app =
    function
    | "Windows" -> $"{app.Name}.exe"
    | _ -> app.Name

let zipAndUploadAsset app =
    stage $"Zip And Upload {app.Name} Release Asset" {
        stage "Zip" {
            workingDir $"{app.Path}/publish"

            run (fun ctx ->
                let path = appZipPath app ctx
                let os = ctx.GetEnvVar(args.os.Name)
                let binaryName = publishedBinaryName app os
                zipCmd binaryName path os)

            stage "Set output variable" {
                whenEnvVar args.ci.Name
                run (fun ctx -> bashCmd $"""echo "artifact-path={appZipPath app ctx}" >> $GITHUB_OUTPUT""")
            }

            stage "Upload" {
                workingDir app.Path

                whenCmdArg args.release

                run (fun ctx ->
                    $"gh release upload {ctx.GetCmdArg(args.release)} '{appZipPath app ctx}#{languageServerLabel ctx}'")
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
