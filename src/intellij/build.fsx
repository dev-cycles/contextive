#r "nuget: Fun.Build, 1.0.5"
#load "../ci/common.fsx"

open Fun.Build
open Fun.Build.Github
open Common

let distPath: string = "intellij/contextive/build/distributions"

let intelliJAssetFileName (ctx: Internal.StageContext) =
    $"""contextive-{versionNumber (ctx.GetCmdArg(args.release))}-signed.zip"""

let intelliJAssetLabel (ctx: Internal.StageContext) =
    $"""Contextive IntelliJ Plugin {ctx.GetCmdArg(args.release)}"""

pipeline "Contextive IntelliJ Plugin" {
    description "Build & Test"
    noPrefixForStep
    collapseGithubActionLogs

    logEnvironment

    stage "Publish" {
        workingDir "intellij/contextive"
        whenCmdArg args.release

        stage "Sign Plugin" { run (bashCmd "./gradlew signPlugin") }

        stage "Upload Asset" {
            workingDir distPath

            run (fun ctx -> $"gh release upload {ctx.GetCmdArg(args.release)} {intelliJAssetFileName ctx}")
        }

        stage "Publish Package" {
            whenComponentInRelease "intellij"
            run (bashCmd "./gradlew publishPlugin")
        }
    }

    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
