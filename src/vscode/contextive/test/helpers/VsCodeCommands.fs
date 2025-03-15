module Contextive.VsCodeExtension.Tests.E2E.Helpers.VsCodeCommands

open Fable.Import.VSCode.Vscode
open Fable.Core

let complete docUri position : JS.Promise<CompletionList option> =
    promise { return! commands.executeCommand ("vscode.executeCompletionItemProvider", Some docUri, Some position) }

let hover docUri position : JS.Promise<ResizeArray<Hover option>> =
    promise { return! commands.executeCommand ("vscode.executeHoverProvider", Some docUri, Some position) }

let initialize () =
    promise {
        let! res = commands.executeCommand "contextive.initialize"
        do! Promise.sleep 50
        return res
    }

let closeActiveEditor () =
    promise { return! commands.executeCommand "workbench.action.closeActiveEditor" }

let closeAllEditors () =
    promise { return! commands.executeCommand "workbench.action.closeAllEditors" }

let openFolder docUri =
    promise { return! commands.executeCommand ("vscode.openFolder", Some docUri) }
