#r "nuget: Fun.Build, 1.0.5"
#load "../ci/common.fsx"

open Fun.Build

open Common

let visualStudioProjectPath = "visualstudio/contextive/contextive"

let visualStudioAssetPath =
    $"{visualStudioProjectPath}/bin/Release/net8.0-windows7.0"

let visualStudioAssetFileName = "contextive.vsix"

pipeline "Contextive Visual Studio Extension" {
    description "Build & Test"
    noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    logEnvironment

    stage "Prep Language Server Artifact" {
        stage "Copy when Local" {
            whenNot { envVar args.ci.Name }

            run $"cp {languageServer.Path}/publish/Contextive.LanguageServer ${visualStudioProjectPath}"
        }

        stage "Download Release Asset" {
            whenCmdArg args.release

            run (fun ctx ->
                $"gh release download {ctx.GetCmdArg(args.release)} -p {appZipFileName languageServer ctx} --clobber")
        }

        stage "Unzip in CI" {
            whenEnvVar args.ci.Name

            run (fun ctx ->
                ctx.GetEnvVar(args.os.Name)
                |> unzipCmd (appZipFileName languageServer ctx) visualStudioProjectPath)
        }

    }

    stage "Package" {
        workingDir visualStudioProjectPath

        run "dotnet publish"
    }

    stage "Publish" {
        workingDir visualStudioAssetPath
        whenCmdArg args.release

        stage "Upload Asset" {
            run (fun ctx -> $"gh release upload {ctx.GetCmdArg(args.release)} {visualStudioAssetFileName}")
        }

    //stage "Publish to Marketplace" { run (fun ctx -> $"npx vsce publish --packagePath {vsCodeAssetFileName ctx}") }
    }

    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
