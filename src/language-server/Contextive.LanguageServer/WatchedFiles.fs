module Contextive.LanguageServer.WatchedFiles

open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open System.Threading.Tasks

let handler (onChangedHandler: SubGlossary.OnChangedHandler) (p: DidChangeWatchedFilesParams) _ _ =
    onChangedHandler ()
    Task.CompletedTask

let private registrationOptionsProvider (path: string option) _ _ =
    match path with
    | Some pathValue ->
        DidChangeWatchedFilesRegistrationOptions(
            Watchers = Container(FileSystemWatcher(GlobPattern = pathValue, Kind = WatchKind.Create + WatchKind.Change))
        )
    | None -> DidChangeWatchedFilesRegistrationOptions()

let registrationOptions path =
    RegistrationOptionsDelegate<DidChangeWatchedFilesRegistrationOptions, DidChangeWatchedFilesCapability>(
        registrationOptionsProvider path
    )

let register (s: ILanguageServer) onChangedHandler fullPath =
    let registration =
        s.Register(fun reg ->
            reg.OnDidChangeWatchedFiles(handler <| onChangedHandler, registrationOptions fullPath)
            |> ignore)

    fun () -> registration.Dispose()


let nHandler (watchedFileChangedHandlers: Glossary.OnWatchedFilesEventHandlers) (p: DidChangeWatchedFilesParams) _ _ =
    //p.Changes |> Seq.head |> (fun s -> s.Type = Chan)
    Task.CompletedTask

let nRegister (s: ILanguageServer) (watchedFileChangedHandlers: Glossary.OnWatchedFilesEventHandlers) glob =
    let registration =
        s.Register(fun reg ->
            reg.OnDidChangeWatchedFiles(nHandler watchedFileChangedHandlers, registrationOptions glob)
            |> ignore)

    fun () -> registration.Dispose()
