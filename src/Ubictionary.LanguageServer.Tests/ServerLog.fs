module Ubictionary.LanguageServer.Tests.ServerLog

open System.Threading.Tasks
open OmniSharp.Extensions.LanguageServer.Protocol.Models

type LogHandler = LogMessageParams -> Task

let createHandler logAwaiter:LogHandler =
    fun (l:LogMessageParams) ->
        l.Message |> ConditionAwaiter.received logAwaiter 
        Task.CompletedTask

let waitForLogMessage logAwaiter (logMessage:string) = async {
    let logCondition = fun (m:string) -> m.Contains(logMessage)

    return! ConditionAwaiter.waitFor logAwaiter logCondition 1500
}

