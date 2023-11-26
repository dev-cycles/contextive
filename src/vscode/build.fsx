#r "nuget: Fun.Build, 1.0.5"
#load "../ci/common.fsx"

open Fun.Build

open Common

let distPath = "vscode/contextive/dist"

let vsCodeAssetFileName (ctx: Internal.StageContext) =
    $"contextive-{ctx.GetCmdArg(args.vscePlatform)}-{ctx.GetCmdArg(args.release)}.vsix"

pipeline "Contextive VsCode Extension" {
    description "Build & Test"
    noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    logEnvironment

    stage "Install Tools" {
        whenEnv { name args.ci.Name }
        paralle
        installTool "paket"
        installTool "fable"
    }

    stage "Npm Install" {
        workingDir "vscode/contextive"
        paralle
        run "npm install"
        run "paket restore"
    }

    stage "Start Xvbf" {
        run (bashCmd "/usr/bin/Xvfb :99 -screen 0 1024x768x24 > /dev/null 2>&1 &")
        echo ">>> Started xvfb"
    }

    stage "Prep Language Server Artifact" {
        stage "Copy when Local" {
            whenNot { envVar args.ci.Name }

            run $"cp {languageServer.Path}/publish/Contextive.LanguageServer vscode/contextive/dist"
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
                |> unzipCmd (appZipFileName languageServer ctx) "vscode/contextive/dist")
        }

        stage "Make Language Server Executable" {
            whenAny {
                platformLinux
                platformOSX
            }

            run "chmod a+x vscode/contextive/dist/Contextive.LanguageServer"
        }

    }

    stage "Test" {
        workingDir "vscode/contextive"

        // All github runners are x64, so the arm64 runtimes don't execute on them
        // and we can only run tests with the x64 runtime builds.
        whenAny {
            whenCmd {
                name args.dotnetRuntime.Name.Names.Head
                acceptValues [ "linux-x64"; "osx-x64"; "win-x64" ]
            }

            whenNot { whenEnv { name args.ci.Name } }
        }

        envVars [ "DISPLAY", ":99.0" ]
        run "npm test"
    }

    stage "Publish" {
        workingDir "vscode/contextive"
        whenCmdArg args.release
        whenCmdArg args.vscePlatform

        stage "Package" {
            run (fun ctx ->
                $"npx vsce package -t {ctx.GetCmdArg(args.vscePlatform)} --githubBranch main --baseImagesUrl https://raw.githubusercontent.com/dev-cycles/contextive/{ctx.GetCmdArg(args.release)}/src/vscode/contextive/")
        }

        stage "Upload Asset" {
            run (fun ctx -> $"gh release upload {ctx.GetCmdArg(args.release)} {vsCodeAssetFileName ctx}")
        }

        stage "Publish to Marketplace" { echo "Not doing yet." }
    }

    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
