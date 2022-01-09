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
    let private executable f = jsOptions<Executable>(f)
    let private executableOptions f = Some <| jsOptions<ExecutableOptions>(f)
    let private argsArray (f:string list) = Some <| new ResizeArray<string>(f)
    let private documentSelectorList (x:string list) = Some !^(new ResizeArray<string>(x))

    let private debugServerOptions = executable(fun x -> 
        x.command <- "dotnet"
        x.args <- argsArray ["run"]
        x.options <- executableOptions(fun x -> 
            x.cwd <- Some <| path.resolve(__dirname, "../../../Contextly.LanguageServer")
        )
    )

    let private runServerOptions = executable(fun x -> 
        x.command <- "./Contextly.LanguageServer"
        x.options <- executableOptions(fun x -> 
            x.cwd <- Some <| path.resolve(__dirname)
        )
    )

    let private serverOptions =
        createObj [
            "run" ==> runServerOptions
            "debug" ==> debugServerOptions
        ] |> unbox<ServerOptions>
          
    let private clientOptions = languageClientOptions(fun x ->
        x.documentSelector <- documentSelectorList ["plaintext"; "markdown"; "yaml"]
    )

    let private clientFactory() =
        LanguageClient("contextly",
            "Contextly",
            serverOptions=serverOptions,
            clientOptions=clientOptions,
            forceDebug=(process.env?CONTEXTLY_CI="true")
        )

    type Api = {
        Client: LanguageClient
    }

    let private addDisposable (context: ExtensionContext) (disposable:Disposable) =
        context.subscriptions.Add(disposable :?> ExtensionContextSubscriptions)

    let private registerCommand (context: ExtensionContext) (name: string) (handler) =
        commands.registerCommand(name, handler) |> addDisposable context 
    
    let private getPath() =
        let config = workspace.getConfiguration("contextly")
        match config.get("path"), workspace.workspaceFolders with
        | Some p, Some wsf -> vscode.Uri.joinPath(wsf[0].uri, [|p|]) |> Some
        | _, _ -> None

    let activate (context: ExtensionContext) = promise {
        let client = clientFactory()
        
        client.start() |> addDisposable context

        do! client.onReady()

        Initialize.handler getPath |> registerCommand context "contextly.initialize"

        // May be needed if we refactor towards using the language server to initialize files - can remove 
        // after that is resolved
        // context.subscriptions.Add(workspace.onDidChangeConfiguration.Invoke(fun e -> 
        //     if e.affectsConfiguration("contextly") then
        //         client.sendNotification("workspace/didChangeConfiguration", Some {| settings = None |})
        //     None
        // ) :?> ExtensionContextSubscriptions)

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