module Contextive.LanguageServer.RemoteFileReader

open FSharp.Data
open Contextive.Core.File

let private getText =
    function
    | Text t -> Ok t
    | Binary b ->
        $"Expecting text, but got a binary response ({b.Length} bytes)"
        |> ReadingError
        |> Error

let private statusCodeToFileError statusCode b =
    match statusCode with
    | HttpStatusCodes.OK -> b |> Ok
    | HttpStatusCodes.NotFound -> FileNotFound |> Error
    | c -> $"HttpStatusCode:{c}, payload: '{b}'" |> ReadingError |> Error

let read u : Result<string, FileError> =
    try
        let response = Http.Request(u, silentHttpErrors = true)
        getText response.Body |> Result.bind (statusCodeToFileError response.StatusCode)
    with ex ->
        ex.Message |> ReadingError |> Error
