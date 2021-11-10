module Ubictionary.LanguageServer.Tests.Workspace

open System.Threading.Tasks
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Client

let private createHandler path =
    fun (p:WorkspaceFolderParams) ->
        Task.FromResult(Container(WorkspaceFolder(Uri = DocumentUri.FromFileSystemPath(
            Path.Combine(Directory.GetCurrentDirectory(),path)
        ))))

let createOptionsBuilder path =
    let handler = createHandler path

    fun (b:LanguageClientOptions) ->
        b.EnableWorkspaceFolders()
            .OnWorkspaceFolders(handler)
            |> ignore
        b