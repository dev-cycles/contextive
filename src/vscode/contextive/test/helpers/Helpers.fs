module Contextive.VsCodeExtension.Tests.E2E.Helpers.Helpers

open Contextive.VsCodeExtension
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Node.Api
open Fable.Core
open Fable.Mocha

let util: obj = JsInterop.importAll "node:util"

[<Emit("util.inspect($0, { showHidden: true, depth: null, getters: true })")>]
let inspect (_: obj) = jsNative

let logInspect o = JS.console.log (inspect (o))

let getDocUri relativeFile =
    vscode.Uri.file (path.resolve (__dirname, relativeFile))

let findUriInVisibleEditors (docUri: Uri) =
    window.visibleTextEditors.Find(fun te ->
        // We compare the .toString() for platform compatibility
        // On windows, the drive letter was captilised in the docUri, not in the textEditor
        te.document.uri.toString () = docUri.toString ())

let documentIsOpen docUri () =
    promise {
        let editor = findUriInVisibleEditors (docUri)
        return editor <> null
    }

let openDocument (docUri: Uri) =
    promise {
        let! doc = workspace.openTextDocument (docUri)
        do! window.showTextDocument (doc, ViewColumn.Active, false) |> Thenable.Ignore
        do! Waiter.waitFor <| documentIsOpen docUri
        return docUri
    }

let closeDocument (docUri: Uri) =
    promise {
        let! doc = workspace.openTextDocument (docUri)
        do! window.showTextDocument (doc, ViewColumn.Active, false) |> Thenable.Ignore

        do!
            commands.executeCommand ("workbench.action.closeActiveEditor")
            |> Thenable.Ignore

        return ()
    }

let getContentBuffer (content: string) =
    System.Text.Encoding.UTF8.GetBytes(content)
    |> Fable.Core.JS.Constructors.Uint8Array.Create

let writeFile (docUri: Uri) (content: string) =
    promise {
        do! workspace.fs.writeFile (docUri, getContentBuffer content)
        do! Promise.sleep 400
    }

let pathInWorkspace path =
    let wsf = workspace.workspaceFolders.Value[0]
    vscode.Uri.joinPath (wsf.uri, [| path |])

let getConfig () =
    workspace.getConfiguration ("contextive")

let getDefaultPath () =
    pathInWorkspace ".contextive/definitions.yml"

let getFullPathFromConfig () =
    let config = getConfig ()
    config["path"].Value :?> string |> pathInWorkspace

let updateConfig newPath =
    promise {
        let config = getConfig ()
        do! config.update ("path", newPath :> obj |> Some)
        do! Promise.sleep 50
    }

let resetConfig () =
    promise {
        let config = getConfig ()
        do! config.update ("path", None)
    }

let deleteFile fullPath =
    promise {
        do! closeDocument fullPath |> Promise.Ignore
        do! workspace.fs.delete fullPath
    }

let deleteConfiguredDefinitionsFile () =
    promise { do! deleteFile <| getFullPathFromConfig () }

[<Emit("afterEach($0, $1)")>]
let afterEach (name: string, callback: unit -> JS.Promise<unit>) : unit = jsNative

[<Emit("after($0, $1)")>]
let after (name: string, callback: unit -> JS.Promise<unit>) : unit = jsNative

[<Emit("beforeEach($0, $1)")>]
let beforeEach (name: string, callback: unit -> JS.Promise<unit>) : TestCase = jsNative

[<Emit("before($0, $1)")>]
let before (name: string, callback: unit -> JS.Promise<unit>) : TestCase = jsNative
