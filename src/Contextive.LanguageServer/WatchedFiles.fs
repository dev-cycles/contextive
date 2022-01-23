module Contextive.LanguageServer.WatchedFiles

open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open System.Threading.Tasks

let handler (reloader: Definitions.Reloader) (p:DidChangeWatchedFilesParams) _ _ = 
    reloader()
    Task.CompletedTask

let private registrationOptionsProvider path _ _ =
    match path with
    | Some pathValue -> 
        DidChangeWatchedFilesRegistrationOptions(
            Watchers = Container(
                FileSystemWatcher(
                    GlobPattern = pathValue,
                    Kind = WatchKind.Change),
                FileSystemWatcher(
                    GlobPattern = pathValue,
                    Kind = WatchKind.Create)
        ))
    | None -> DidChangeWatchedFilesRegistrationOptions()

let registrationOptions path =
    RegistrationOptionsDelegate<DidChangeWatchedFilesRegistrationOptions, DidChangeWatchedFilesCapability>(registrationOptionsProvider path)

let register (s:ILanguageServer) reloader fullPath = 
    let registration = s.Register(fun reg -> 
        reg.OnDidChangeWatchedFiles(handler <| reloader, registrationOptions fullPath)
        |> ignore)
    fun () -> registration.Dispose()