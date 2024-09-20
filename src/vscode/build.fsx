#load "../ci/common.fsx"

open Fun.Build
open Fun.Build.Github
open Common

let vsCodeAssetFileName (ctx: Internal.StageContext) =
    $"""contextive-{ctx.GetCmdArg(args.vscePlatform)}-{versionNumber (ctx.GetCmdArg(args.release))}.vsix"""

let vsCodeAssetLabel (ctx: Internal.StageContext) =
    $"Contextive VsCode Extension {ctx.GetCmdArg(args.release)} ({ctx.GetCmdArg(args.vscePlatform)})"

let vsCodePath = "vscode/contextive"

let publishTag (ctx: Internal.StageContext) = 
    match ctx.GetCmdArg(args.release) with
    | "" -> ctx.GetEnvVar(args.headSha.Name)
    | r -> r

pipeline "Contextive VsCode Extension" {
    description "Build & Test"
    noPrefixForStep
    collapseGithubActionLogs
    
    logEnvironment

    stage "Install Tools" {
        whenEnv { name args.ci.Name }
        paralle
        dotnetRestoreTools
    }

    stage "Npm Install" {
        workingDir vsCodePath
        paralle
        run "npm install"
        run "dotnet paket restore"
    }

    stage "Start Xvbf & Dbus" {

        run (bashCmd "/usr/bin/Xvfb :99 -screen 0 1024x768x24 > /dev/null 2>&1 &")

        stage "Start dbus" {
            whenLinux
            run (bashCmd "sudo service dbus start")
            run (bashCmd "sudo mkdir -p $XDG_RUNTIME_DIR")
            run (bashCmd "sudo chmod 700 $XDG_RUNTIME_DIR")
            run (bashCmd "sudo chown $(id -un):$(id -gn) $XDG_RUNTIME_DIR")

            run (
                bashCmd "dbus-daemon --session --address=$DBUS_SESSION_BUS_ADDRESS --nofork --nopidfile --syslog-only &"
            )
        }

        echo ">>> Started xvfb & dbus"
    }

    stage "Prep Environment" {
        workingDir vsCodePath

        run "rm -fv .vscode-test/user-data/Crashpad/pending/*"
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
        workingDir vsCodePath

        whenAny {
            whenCmd {
                name args.dotnetRuntime.Name.Names.Head
                acceptValues [ "linux-x64"; "osx-x64"; "win-x64"; "osx-arm64" ]
            }

            whenNot { whenEnv { name args.ci.Name } }
        }

        envVars [ "DISPLAY", ":99.0" ]
        run "npm test"
    }

    
    stage "Prepublish" {
        workingDir vsCodePath
        whenCmdArg args.vscePlatform

        stage "Package" {
            run (fun ctx ->
                $"npx vsce package -t {ctx.GetCmdArg(args.vscePlatform)} --githubBranch {publishTag ctx} --baseContentUrl https://github.com/dev-cycles/contextive/blob/{publishTag ctx}/src/vscode/contextive/ --baseImagesUrl https://raw.githubusercontent.com/dev-cycles/contextive/{publishTag ctx}/src/vscode/contextive/")
        }
    }

    stage "Publish" {
        workingDir vsCodePath
        whenCmdArg args.release
        whenCmdArg args.vscePlatform

        stage "Upload Asset" {
            run (fun ctx -> $"gh release upload {ctx.GetCmdArg(args.release)} {vsCodeAssetFileName ctx}")
        }

        stage "Publish to Marketplaces" {
            whenComponentInRelease "vscode" "../.."
            
            stage "To Microsoft VS Marketplace" { run (fun ctx -> $"npx vsce publish --packagePath {vsCodeAssetFileName ctx}") }
            stage "To Open-Vsx Marketplace" { run (fun ctx -> $"npx ovsx publish {vsCodeAssetFileName ctx} -p \"{ctx.GetEnvVar(args.ovsxPat.Name)}\"") }            
        }
    }

    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
