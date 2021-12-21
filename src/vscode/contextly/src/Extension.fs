namespace Contextly.VsCodeExtension

open Fable.Core.JsInterop
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Fable.Import.LanguageServer
open Fable.Import.LanguageServer.Client
open Node.Api

module Extension =

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

    let private client =
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
        client.start() |> ignore
        let registerCommandInContext = registerCommand context
        match getPath() with
        | Some p -> registerCommandInContext "contextly.initialize" (Initialize.handler p)
        | None -> window.showErrorMessage("Please open a workspace before initializing Contextly.") |> ignore
        return {
            Client = client
        }
    }

    let deactivate () = promise {
        do! client.stop()
    }