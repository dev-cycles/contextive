#r "nuget: Fun.Build, 1.0.5"
#load "../ci/common.fsx"

open Fun.Build

open Common

let distPath: string = "intellij/contextive/build/distribution"

let intelliJAssetFileName (ctx: Internal.StageContext) =
    $"contextive-{ctx.GetCmdArg(args.release)}-signed.zip"

pipeline "Contextive IntelliJ Plugin" {
    description "Build & Test"
    noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    logEnvironment

    stage "Publish" {
        workingDir "intellij/contextive"
        whenCmdArg args.release

        stage "Sign Plugin" { run "./gradlew signPlugin" }

        stage "Upload Asset" {
            workingDir distPath
            run (fun ctx -> $"gh release upload {ctx.GetCmdArg(args.release)} {intelliJAssetFileName ctx}")
        }

        stage "Publish Package" { run "./gradlew publishPlugin" }
    }

    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
