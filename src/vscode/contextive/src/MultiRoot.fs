module Contextive.VsCodeExtension.MultiRoot

open Middleware
open Fable.Core.JsInterop
open Fable.Import.VSCode.Vscode
open Fable.Import.LanguageServer.Client
open Node.Api

let private getMultiRootRelativePath configuredPath =
    match workspace.workspaceFile with
    | Some (wsFileUri) -> 
            path.join(wsFileUri.fsPath |> path.dirname, configuredPath)
    | None -> configuredPath

let getPath (result:ResizeArray<obj>) =
    result[0]?path

let private configExists (result:ResizeArray<obj>) =
    result.Count > 0

let private isConfiguredPathAbsolute (result:ResizeArray<obj>) =
    result |> getPath |> path.isAbsolute

let private pathNeedsRewriting (result:ResizeArray<obj>) =
    configExists result && not <| isConfiguredPathAbsolute result

let private multiRootMiddleware (params:ConfigurationParams) (_:CancellationToken) (next:ConfigFunc) = 
        let result = next params
        if pathNeedsRewriting result then
            result[0]?path <- result |> getPath |> getMultiRootRelativePath
        result  

let private configurationMiddleware = {
    configuration = multiRootMiddleware
}

let middleware:Middleware = 
    { workspace = configurationMiddleware }