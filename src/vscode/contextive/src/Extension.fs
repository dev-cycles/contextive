module Contextive.VsCodeExtension.Extension

open Fable.Core.JsInterop
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Fable.Import.LanguageServer
open Fable.Import.LanguageServer.Client
open Node.Api

exception NoExtensionApi

let private ExtensionId = "contextive"

let private languageClientOptions = jsOptions<LanguageClientOptions>
let private executable f = jsOptions<Executable> (f)

let private executableOptions f =
    Some <| jsOptions<ExecutableOptions> (f)

let private argsArray (f: string list) = Some <| new ResizeArray<string>(f)

let private debugServerOptions =
    executable (fun x ->
        x.command <- "dotnet"
        x.args <- argsArray [ "run" ]

        x.options <-
            executableOptions (fun x ->
                x.cwd <-
                    Some
                    <| path.resolve (__dirname, "../../../language-server/Contextive.LanguageServer")))

let private runServerOptions =
    executable (fun x ->
        x.command <- "./Contextive.LanguageServer"
        x.options <- executableOptions (fun x -> x.cwd <- Some <| path.resolve (__dirname)))

let private serverOptions =
    createObj [ "run" ==> runServerOptions; "debug" ==> debugServerOptions ]
    |> unbox<ServerOptions>

let private clientOptions =
    languageClientOptions (fun x ->
        x.documentSelector <- Some !! "*"
        x.middleware <- Some MultiRoot.middleware)

let private clientFactory () =
    LanguageClient(
        ExtensionId,
        "Contextive",
        serverOptions = serverOptions,
        clientOptions = clientOptions,
        forceDebug = Option.isSome process.env?CONTEXTIVE_DEBUG
    )

type Api = { Client: LanguageClient }

let private addDisposable (context: ExtensionContext) (disposable: Disposable) = context.subscriptions.Add(!!disposable)

let private registerCommand (context: ExtensionContext) (name: string) (handler) =
    commands.registerCommand (name, handler) |> addDisposable context

let registerConfigChangeNotifications context (client: LanguageClient) =
    workspace.onDidChangeConfiguration.Invoke(fun e ->
        if e.affectsConfiguration (ExtensionId) then
            client.sendNotification ("workspace/didChangeConfiguration", Some {| settings = None |})

        None)
    |> addDisposable context

let activate (context: ExtensionContext) =
    promise {
        let client = clientFactory ()

        do! client.start ()

        Initialize.handler client |> registerCommand context "contextive.initialize"

        registerConfigChangeNotifications context client

        return { Client = client }
    }

let rec getLanguageClient () =
    let extension = extensions.all.Find(fun x -> x.id = "devcycles.contextive")

    match extension.exports with
    | Some e ->
        let extensionApi: Api = !!e
        extensionApi.Client
    | None -> raise NoExtensionApi


let deactivate () =
    promise {
        let languageClient = getLanguageClient ()
        do! languageClient.stop ()
    }
