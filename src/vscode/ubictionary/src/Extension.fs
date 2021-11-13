namespace Ubictionary.VsCodeExtension

open Fable.Core
open Fable.Core.JsInterop
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

    let private serverPath = Some <| path.resolve(__dirname, "../../../Ubictionary.LanguageServer")

    let private serverOptions = executable(fun x -> 
        x.command <- "dotnet"
        x.args <- argsArray ["run"]
        x.options <- executableOptions(fun x -> 
            x.cwd <- serverPath
        )
    )
          
    let private clientOptions = languageClientOptions(fun x ->
            x.documentSelector <- documentSelectorList ["plaintext"; "markdown"]
        )

    let private client =
        LanguageClient("ubictionary",
            "Ubictionary",
            serverOptions=serverOptions,
            clientOptions=clientOptions,
            forceDebug = false
        )

    type Api = {
        Client: LanguageClient
    }

    let activate (context: ExtensionContext) = promise {
        client.start() |> ignore
        return {
            Client = client
        }
    }

    let deactibate () = promise {
        do! client.stop()
    }