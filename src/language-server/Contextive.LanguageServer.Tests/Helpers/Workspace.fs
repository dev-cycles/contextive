module Contextive.LanguageServer.Tests.Helpers.Workspace

open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Client

let workspaceFolderPath relativePath =
    DocumentUri.FromFileSystemPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath))

let optionsBuilder path (b: LanguageClientOptions) =
    let wsPath = workspaceFolderPath path
    b.WithWorkspaceFolder(wsPath, "Default")

let rootOptionsBuilder path (b: LanguageClientOptions) =
    let wsPath = workspaceFolderPath path
    b.WithRootUri(wsPath)
