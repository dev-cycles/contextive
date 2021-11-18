module Ubictionary.LanguageServer.Tests.Workspace

open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Client

let optionsBuilder path (b:LanguageClientOptions) =
    let workspaceFolderPath = DocumentUri.FromFileSystemPath(Path.Combine(Directory.GetCurrentDirectory(),path))
    b.WithWorkspaceFolder(workspaceFolderPath, "Default")