module Ubictionary.LanguageServer.Tests.Workspace

open System.Threading.Tasks
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace

let private createHandler path (p:WorkspaceFolderParams) =
    Task.FromResult(Container(WorkspaceFolder(Uri = DocumentUri.FromFileSystemPath(
        Path.Combine(Directory.GetCurrentDirectory(),path)
    ))))

let optionsBuilder path (b:LanguageClientOptions) =
    let handler = createHandler path
    b.EnableWorkspaceFolders()
        .OnWorkspaceFolders(handler)
        |> ignore
    b
