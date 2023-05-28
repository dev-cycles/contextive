module Contextive.VsCodeExtension.Middleware

open Fable.Import.VSCode.Vscode
open Fable.Import.LanguageServer.Client

type ConfigurationItem = {
    scopeUri: string option
    section: string option
}

type ConfigurationParams = {
    items: ResizeArray<ConfigurationItem>
}

type ConfigFunc = ConfigurationParams -> ResizeArray<obj>

type private MiddlewareFunction = ConfigurationParams -> CancellationToken -> ConfigFunc -> ResizeArray<obj>

type ConfigurationMiddleware = {
    configuration: MiddlewareFunction
}

type WorkspaceConfigurationMiddleware =
    {
        workspace: ConfigurationMiddleware
    } 
    interface Middleware