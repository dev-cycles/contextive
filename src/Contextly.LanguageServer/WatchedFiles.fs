module Contextly.LanguageServer.WatchedFiles

open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open System.Threading.Tasks

let handler (reloader: Definitions.Reloader) (p:DidChangeWatchedFilesParams) _ _ = 
    async {
        do! reloader() |> Async.Ignore
        return ()
    } |> Async.StartAsTask :> Task

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

let register (s:ILanguageServer) fullPath reloader = 
    s.Register(fun reg -> 
        reg.OnDidChangeWatchedFiles(handler <| reloader, registrationOptions fullPath)
        |> ignore) |> ignore