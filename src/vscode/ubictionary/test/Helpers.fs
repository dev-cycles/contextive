module Ubictionary.VsCodeExtension.Tests.Helpers

open Fable.Core.JsInterop
open Fable.Import.VSCode.Vscode
open Fable.Import.LanguageServer
open Ubictionary.VsCodeExtension.Extension

/// Waits for the active language client to be ready
let getLanguageClient() = promise {
    let extension = extensions.all.Find(fun x -> x.id = "devcycles.ubictionary")
    let extensionApi: Api = !!extension.exports.Value
    let languageClient:LanguageClient = extensionApi.Client
    do! languageClient.onReady()
    return languageClient
}