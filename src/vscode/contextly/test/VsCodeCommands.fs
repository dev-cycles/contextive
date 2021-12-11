module Contextly.VsCodeExtension.Tests.VsCodeCommands

open Fable.Import.VSCode.Vscode

let completion docUri position = promise {
    return! commands.executeCommand(
                "vscode.executeCompletionItemProvider",
                Some (docUri :> obj),
                Some (position :> obj)
            )
}