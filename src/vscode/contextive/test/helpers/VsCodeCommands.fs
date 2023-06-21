module Contextive.VsCodeExtension.Tests.E2E.Helpers.VsCodeCommands

open Fable.Import.VSCode.Vscode
open Fable.Core.JS

let complete docUri position : Promise<CompletionList option> =
    promise { return! commands.executeCommand ("vscode.executeCompletionItemProvider", Some docUri, Some position) }

let hover docUri position : Promise<ResizeArray<Hover option>> =
    promise { return! commands.executeCommand ("vscode.executeHoverProvider", Some docUri, Some position) }

let initialize () =
    promise { return! commands.executeCommand ("contextive.initialize") }

let closeActiveEditor () =
    promise { return! commands.executeCommand ("workbench.action.closeActiveEditor") }

let closeAllEditors () =
    promise { return! commands.executeCommand ("workbench.action.closeAllEditors") }

let openFolder docUri =
    promise { return! commands.executeCommand ("vscode.openFolder", Some docUri) }
