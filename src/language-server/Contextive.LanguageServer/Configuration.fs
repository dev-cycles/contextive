module Contextive.LanguageServer.Configuration

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol.Server

let resolvedPathGetter configGetter pathResolver () =
    async {
        let! path = configGetter ()
        return pathResolver path
    }

let handler onDefaultGlossaryLocationChanged _ =
    onDefaultGlossaryLocationChanged ()
    Task.CompletedTask

let getWorkspaceFolder (s: ILanguageServer) =
    let workspaceFolders = s.WorkspaceFolderManager.CurrentWorkspaceFolders

    if not (Seq.isEmpty workspaceFolders) then
        let workspaceRoot = workspaceFolders |> Seq.head
        Some <| workspaceRoot.Uri.ToUri().LocalPath
    else if s.Client.ClientSettings.RootUri <> null then
        Some <| s.Client.ClientSettings.RootUri.ToUri().LocalPath
    else
        None

let getWorkspaceFolders (s: ILanguageServer) =
    let workspaceRoots =
        s.WorkspaceFolderManager.CurrentWorkspaceFolders
        |> Seq.map _.Uri.ToUri().LocalPath

    if not <| Seq.isEmpty workspaceRoots then
        workspaceRoots
    else if s.Client.ClientSettings.RootUri <> null then
        seq { s.Client.ClientSettings.RootUri.ToUri().LocalPath }
    else
        Seq.empty
