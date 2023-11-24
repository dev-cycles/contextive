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
    System.IO.Path.GetFullPath($"{app.Name}-{ctx.GetCmdArg(args.dotnetRuntime)}-{ctx.GetEnvVar(args.refName.Name)}.zip")

let checkRelease app =
    stage "Check Release" {
        workingDir $"{app.Path}/publish"

        whenLinux

        acceptExitCodes [ 124 ]

        run (bashCmd $"timeout -v 2 ./{app.Name}")
    }

let zipAndUploadAsset app =
    stage $"Zip And Upload {app.Name} Release Asset" {
        stage "Zip" {
            workingDir $"{app.Path}/publish"

            run (fun ctx ->
                let path = appZipFileName app ctx

                let zipCmd =
                    match ctx.GetEnvVar(args.os.Name) with
                    | "Linux"
                    | "" -> "zip -v"
                    | _ -> "7z a"

                $"{zipCmd} {path} * ")

            stage "Upload" {
                workingDir app.Path

                whenAny {
                    cmdArg args.refName.Name
                    envVar args.refName.Name
                }

                run (fun ctx ->
                    bashCmd $"gh release upload {ctx.GetCmdArgOrEnvVar(args.refName.Name)} {appZipFileName app ctx}")
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
