module Contextly.VsCodeExtension.Tests.Helpers

open Fable.Core.JsInterop
open Fable.Import.LanguageServer
open Contextly.VsCodeExtension.Extension
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Node.Api

let getDocUri relativeFile =
    vscode.Uri.file(path.resolve(__dirname,relativeFile))

let openDocument (docUri:Uri) = promise {
    let! doc = workspace.openTextDocument(docUri)
    let! _ = window.showTextDocument(doc, ViewColumn.Active, false)
    return docUri
}

let closeDocument (docUri:Uri) = promise {
    let! doc = workspace.openTextDocument(docUri)
    let! _ = window.showTextDocument(doc, ViewColumn.Active, false)
    let! _ = commands.executeCommand("workbench.action.closeActiveEditor")
    return ()
}

/// Waits for the active language client to be ready
let getLanguageClient() = promise {
    let extension = extensions.all.Find(fun x -> x.id = "devcycles.contextly")
    let extensionApi: Api = !!extension.exports.Value
    let languageClient: LanguageClient = extensionApi.Client
    do! languageClient.onReady()

    // This initial request ensures that the server is _really_ ready
    // Without it, we finding that completion responses via the vscode command weren't including
    // this language server's response.
    let rec warmUp() = promise {
        try
            do! languageClient.sendRequest("textDocument/completion", {| |}) |> Promise.Ignore
        with
        | e -> 
            printf "Exception (retrying): %A" e
            do! warmUp()
    }

    do! warmUp()

    return languageClient
}