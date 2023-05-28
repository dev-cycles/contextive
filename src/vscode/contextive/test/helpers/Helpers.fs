module Contextive.VsCodeExtension.TestHelpers.Helpers

open Contextive.VsCodeExtension
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Node.Api
open Fable.Core
open Fable.Mocha

let getDocUri relativeFile =
    vscode.Uri.file(path.resolve(__dirname,relativeFile))

let openDocument (docUri:Uri) = promise {
    let! doc = workspace.openTextDocument(docUri)
    do! window.showTextDocument(doc, ViewColumn.Active, false) |> Thenable.Ignore
    return docUri
}

let closeDocument (docUri:Uri) = promise {
    let! doc = workspace.openTextDocument(docUri)
    do! window.showTextDocument(doc, ViewColumn.Active, false) |> Thenable.Ignore
    do! commands.executeCommand("workbench.action.closeActiveEditor") |> Thenable.Ignore
    return ()
}

let getConfig() = workspace.getConfiguration("contextive")

let getFullPathFromConfig() =
    let config = getConfig()
    let path = config["path"].Value :?> string
    let wsf = workspace.workspaceFolders.Value[0]
    vscode.Uri.joinPath(wsf.uri, [|path|])

let findUriInVisibleEditors (path:Uri) =
    window.visibleTextEditors.Find(fun te -> te.document.uri.path = path.path)

let updateConfig newPath = promise {
    let config = getConfig()
    do! config.update("path", newPath :> obj |> Some)
    do! Promise.sleep 10
}

let resetConfig() = promise {
    let config = getConfig()
    do! config.update("path", None)
}

let deleteDefinitionsFile() = promise {
    let fullPath = getFullPathFromConfig()
    do! closeDocument fullPath |> Promise.Ignore
    do! workspace.fs.delete fullPath
}

let util: obj = JsInterop.importAll "node:util"

[<Emit("util.inspect($0, { showHidden: true, depth: null, getters: true })")>]
let inspect (_: obj) = jsNative

let logInspect o = JS.console.log(inspect(o))


[<Emit("afterEach($0, $1)")>]
let afterEach (name: string, callback: unit -> JS.Promise<unit>):unit = jsNative

[<Emit("after($0, $1)")>]
let after (name: string, callback: unit -> JS.Promise<unit>):unit = jsNative

[<Emit("beforeEach($0, $1)")>]
let beforeEach (name: string, callback: unit -> JS.Promise<unit>):TestCase = jsNative

[<Emit("before($0, $1)")>]
let before (name: string, callback: unit -> JS.Promise<unit>):TestCase = jsNative