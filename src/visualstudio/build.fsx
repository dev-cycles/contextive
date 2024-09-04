#r "nuget: Fun.Build, 1.0.5"
#load "../ci/common.fsx"

open Fun.Build

open Common

let visualStudioProjectPath = "visualstudio/contextive/contextive"

let visualStudioAssetRelativePath = "bin/Release/net8.0-windows7.0"

let visualStudioAssetPath =
    $"{visualStudioProjectPath}/{visualStudioAssetRelativePath}"

let visualStudioAssetFileName = "contextive.vsix"

let vsixPublisherExe = """C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe"""

pipeline "Contextive Visual Studio Extension" {
    description "Build & Test"
    noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    logEnvironment

    stage "Prep Language Server Artifact" {
        stage "Copy when Local" {
            whenNot { envVar args.ci.Name }

            run $"cp {languageServer.Path}/publish/Contextive.LanguageServer {visualStudioProjectPath}"
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
        workingDir visualStudioProjectPath
        whenCmdArg args.release

        stage "Upload Asset" {
            workingDir visualStudioAssetPath
            run (fun ctx -> $"gh release upload {ctx.GetCmdArg(args.release)} {visualStudioAssetFileName}")
        }

        // See https://learn.microsoft.com/en-us/visualstudio/extensibility/walkthrough-publishing-a-visual-studio-extension-via-command-line?view=vs-2022
        stage "Publish to Marketplace" { run (fun ctx ->
            $"\"{vsixPublisherExe}\" publish -payload \"{visualStudioAssetRelativePath}/{visualStudioAssetFileName}\"  -publishManifest \"publishmanifest.json\" -personalAccessToken \"{ctx.GetEnvVar(args.vscePat.Name)}\""
        ) }
    }

    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
