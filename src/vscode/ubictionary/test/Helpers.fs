module Ubictionary.VsCodeExtension.Tests.Helpers

open Fable.Core.JsInterop
open Fable.Import.LanguageServer
open Ubictionary.VsCodeExtension.Extension
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Node.Api

/// Waits for the active language client to be ready
let getLanguageClient() = promise {
    let extension = extensions.all.Find(fun x -> x.id = "devcycles.ubictionary")
    let extensionApi: Api = !!extension.exports.Value
    let languageClient: LanguageClient = extensionApi.Client
    do! languageClient.onReady()

    // This initial request ensures that the server is _really_ ready
    // Without it, we finding that completion responses via the vscode command weren't including
    // this language server's response.
    do! languageClient.sendRequest("textDocument/completion", {| |}) |> Promise.Ignore

    return languageClient
}

let getDocUri relativeFile =
    vscode.Uri.file(path.resolve(__dirname,relativeFile))

let openDocument docPath = promise {
    let docUri = getDocUri docPath
    printfn "%A" (docUri.toString())
    let! doc = workspace.openTextDocument(docUri)
    let! _ = window.showTextDocument(doc, ViewColumn.Active, false)
    return docUri
}