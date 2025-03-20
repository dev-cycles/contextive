#r "nuget: Fun.Build, 1.1.7"
#load "./common.fsx"

open Fun.Build
open Common

let dotnetTest (app: Component) =
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

let publishedBinaryName app =
    function
    | "Windows" -> $"{app.Name}.exe"
    | _ -> app.Name
