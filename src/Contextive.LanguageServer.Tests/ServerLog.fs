module Contextive.LanguageServer.Tests.ServerLog

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Window

type LogHandler = LogMessageParams -> Task

let private createHandler logAwaiter (l:LogMessageParams) =
    l.Message |> ConditionAwaiter.received logAwaiter 
    Task.CompletedTask

let waitForLogMessage logAwaiter (logMessage:string) = async {
    let logCondition = fun (m:string) -> m.Contains(logMessage)

    return! ConditionAwaiter.waitFor logAwaiter logCondition 25000
}

let optionsBuilder logAwaiter (b:LanguageClientOptions) =
    let handler = createHandler logAwaiter
    b.EnableWorkspaceFolders()
        .OnLogMessage(handler)
        |> ignore
    b
