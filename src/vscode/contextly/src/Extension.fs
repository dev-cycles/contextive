namespace Contextly.VsCodeExtension

open Fable.Core.JsInterop
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Fable.Import.LanguageServer
open Fable.Import.LanguageServer.Client
open Node.Api

module Extension =

    exception NoExtensionApi

    let private languageClientOptions = jsOptions<LanguageClientOptions>
    let private executable f = !^jsOptions<Executable>(f)
    let private executableOptions f = Some <| jsOptions<ExecutableOptions>(f)
    let private argsArray (f:string list) = Some <| new ResizeArray<string>(f)
    let private documentSelectorList (x:string list) = Some !^(new ResizeArray<string>(x))

    let private serverPath = Some <| path.resolve(__dirname, "../../../Contextly.LanguageServer")

    let private serverOptions = executable(fun x -> 
        x.command <- "dotnet"
        x.args <- argsArray ["run"]
        x.options <- executableOptions(fun x -> 
            x.cwd <- serverPath
        )
    )
          
    let private clientOptions = languageClientOptions(fun x ->
            x.documentSelector <- documentSelectorList ["plaintext"; "markdown"; "yaml"]
        )

    let private clientFactory() =
        LanguageClient("contextly",
            "Contextly",
            serverOptions=serverOptions,
            clientOptions=clientOptions,
            forceDebug = false
        )

    type Api = {
        Client: LanguageClient
    }

    let private registerCommand (context: ExtensionContext) (name: string) (handler) =
        context.subscriptions.Add(commands.registerCommand(name, handler) :?> ExtensionContextSubscriptions)
    
    let private getPath() =
        let config = workspace.getConfiguration("contextly")
        // match config.get("path") with
        // | Some p -> vscode.Uri.file(p) |> Some
        // | _ -> None
        match config.get("path"), workspace.workspaceFolders with
        | Some p, Some wsf -> vscode.Uri.joinPath(wsf[0].uri, [|p|]) |> Some
        | _, _ -> None

    let activate (context: ExtensionContext) = promise {
        let client = clientFactory()
        context.subscriptions.Add(client.start() :?> ExtensionContextSubscriptions)

        do! client.onReady()

        let registerCommandInContext = registerCommand context

        registerCommandInContext "contextly.initialize" (Initialize.handler getPath)

        context.subscriptions.Add(workspace.onDidChangeConfiguration.Invoke(fun e -> 
            if e.affectsConfiguration("contextly") then
                client.sendNotification("workspace/didChangeConfiguration", Some {| settings = None |})
            None
        ) :?> ExtensionContextSubscriptions)

        return {
            Client = client
        }
    }

    let rec getLanguageClient() = 
        let extension = extensions.all.Find(fun x -> x.id = "devcycles.contextly")
        match extension.exports with
        | Some e -> 
            let extensionApi: Api = !!e
            extensionApi.Client
        | None -> 
            raise NoExtensionApi
    

    let deactivate () = promise {
        let languageClient = getLanguageClient()
        do! languageClient.stop()
    }