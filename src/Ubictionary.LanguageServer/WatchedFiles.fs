module Ubictionary.LanguageServer.WatchedFiles

open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let handler (termFinder: Definitions.Loader) (p:DidChangeWatchedFilesParams) _ _ = 
    ()

let private registrationOptionsProvider path _ _ =
    match path with
    | Some pathValue -> 
        DidChangeWatchedFilesRegistrationOptions(
            Watchers = Container(FileSystemWatcher(
                GlobPattern = pathValue,
                Kind = WatchKind.Change)
        ))
    | None -> DidChangeWatchedFilesRegistrationOptions()

let registrationOptions path =
    RegistrationOptionsDelegate<DidChangeWatchedFilesRegistrationOptions, DidChangeWatchedFilesCapability>(registrationOptionsProvider path)

let register instanceId (s:ILanguageServer) fullPath = 
    s.Register(fun reg -> 
        reg.OnDidChangeWatchedFiles(handler <| Definitions.load instanceId, registrationOptions fullPath)
        |> ignore) |> ignore