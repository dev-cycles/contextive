module Contextive.LanguageServer.WatchedFiles

open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open System.Threading.Tasks

let private registrationOptionsProvider (paths: string array) _ _ =
    let watchers =
        Array.map
            (fun (pathValue: string) ->
                FileSystemWatcher(GlobPattern = pathValue, Kind = WatchKind.Create + WatchKind.Change))
            paths

    DidChangeWatchedFilesRegistrationOptions(Watchers = Container(watchers))

let registrationOptions path =
    RegistrationOptionsDelegate<DidChangeWatchedFilesRegistrationOptions, DidChangeWatchedFilesCapability>(
        registrationOptionsProvider path
    )

let handler
    (watchedFileChangedHandlers: GlossaryManager.OnWatchedFilesEventHandlers)
    (p: DidChangeWatchedFilesParams)
    _
    _
    =
    p.Changes
    |> Seq.iter (fun c ->
        match c.Type with
        | FileChangeType.Created -> watchedFileChangedHandlers.OnCreated <| c.Uri.ToUri().LocalPath
        | FileChangeType.Changed -> watchedFileChangedHandlers.OnChanged <| c.Uri.ToUri().LocalPath
        | _ -> ())

    Task.CompletedTask

let register
    (s: ILanguageServer)
    (watchedFileChangedHandlers: GlossaryManager.OnWatchedFilesEventHandlers)
    (glob: string array)
    =
    let registration =
        s.Register(fun reg ->
            reg.OnDidChangeWatchedFiles(handler watchedFileChangedHandlers, registrationOptions glob)
            |> ignore)

    fun () -> registration.Dispose()
