module Ubictionary.LanguageServer.Tests.Workspace

open System.Threading.Tasks
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models

let createHandler path =
    fun (p:WorkspaceFolderParams) ->
        Task.FromResult(Container(WorkspaceFolder(Uri = DocumentUri.FromFileSystemPath(
            Path.Combine(Directory.GetCurrentDirectory(),path)
        ))))