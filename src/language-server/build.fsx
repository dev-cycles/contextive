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

pipeline languageServer.Name {
    description "Build & Test"
    noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    logEnvironment

    stage "Install Tools" {
        whenEnv { name "GITHUB_EVENT_NAME" }
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

        stage "Check Release" {
            workingDir $"{languageServer.Path}/publish"

            whenEnv {
                name args.os.Name
                value "Linux"
            }

            acceptExitCodes [ 124 ]
            run "bash -c \"timeout -v 2 ./Contextive.LanguageServer\""
        }
    }


    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
