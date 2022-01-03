module Contextly.VsCodeExtension.Tests.Helpers

open Contextly.VsCodeExtension
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Node.Api

let getDocUri relativeFile =
    vscode.Uri.file(path.resolve(__dirname,relativeFile))

let openDocument (docUri:Uri) = promise {
    let! doc = workspace.openTextDocument(docUri)
    do! window.showTextDocument(doc, ViewColumn.Active, false) |> Thenable.Ignore
    return docUri
}

let closeDocument (docUri:Uri) = promise {
    let! doc = workspace.openTextDocument(docUri)
    do! window.showTextDocument(doc, ViewColumn.Active, false) |> Thenable.Ignore
    do! commands.executeCommand("workbench.action.closeActiveEditor") |> Thenable.Ignore
    return ()
}