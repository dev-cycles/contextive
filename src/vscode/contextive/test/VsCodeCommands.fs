module Contextive.VsCodeExtension.Tests.VsCodeCommands

open Fable.Import.VSCode.Vscode

let complete docUri position = promise {
    return! commands.executeCommand(
                "vscode.executeCompletionItemProvider",
                Some (docUri :> obj),
                Some (position :> obj)
            )
}

let initialize() = promise {
    return! commands.executeCommand("contextive.initialize")
}

let closeActiveEditor() = promise {
    return! commands.executeCommand("workbench.action.closeActiveEditor")
}

let closeAllEditors() = promise {
    return! commands.executeCommand("workbench.action.closeAllEditors")
}