#r "nuget: Fun.Build, 1.0.5"
#load "../ci/common.fsx"

open Fun.Build

open Common

let distPath = "vscode/contextive/dist"

pipeline "Contextive VsCode Extension" {
    description "Build & Test"
    noPrefixForStep
    runBeforeEachStage gitHubGroupStart
    runAfterEachStage gitHubGroupEnd

    logEnvironment

    stage "Install Tools" {
        whenEnv { name "GITHUB_EVENT_NAME" }
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
        run "/bin/bash -c \"/usr/bin/Xvfb :99 -screen 0 1024x768x24 > /dev/null 2>&1 &\""
        echo ">>> Started xvfb"
    }

    stage "Prep Language Server Artifact" {
        stage "Copy when Local" {
            whenNot { envVar args.event.Name }

            run "cp language-server/Contextive.LanguageServer/publish/Contextive.LanguageServer vscode/contextive/dist"
        }

        run "chmod a+x vscode/contextive/dist/Contextive.LanguageServer"
    }

    stage "Test" {
        workingDir "vscode/contextive"
        envVars [ "DISPLAY", ":99.0" ]
        run "npm test"
    }

    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()
