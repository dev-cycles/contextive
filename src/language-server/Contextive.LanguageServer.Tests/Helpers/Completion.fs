module Contextive.LanguageServer.Tests.Helpers.Completion

open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open Contextive.LanguageServer

let getLabels (result: CompletionList) = result.Items |> Seq.map (_.Label)

let getCompletionFromText (text: string) (uri: string) (position: Position) (client: ILanguageClient) =
    async {
        client.TextDocument.DidOpenTextDocument(
            DidOpenTextDocumentParams(
                TextDocument = TextDocumentItem(LanguageId = "plaintext", Version = 0, Text = text, Uri = uri)
            )
        )

        let completionParams = CompletionParams(TextDocument = uri, Position = position)

        return!
            client.TextDocument.RequestCompletion(completionParams).AsTask()
            |> Async.AwaitTask
    }

let getRandomUri () =
    $"file:///{System.Guid.NewGuid().ToString()}"

let getCompletion uri =
    getCompletionFromText "" uri <| Position(0, 0)

let getCompletionLabels (client: ILanguageClient) =
    async {
        let! result = getCompletion (getRandomUri ()) client
        return getLabels result
    }

let normalizePathToUri p = System.Uri(p).AbsoluteUri

let getCompletionLabelsInPath (openFileFolder: string) (client: ILanguageClient) =
    async {
        let uri =
            $"{openFileFolder}/{System.Guid.NewGuid().ToString()}" |> normalizePathToUri

        let! result = getCompletion uri client
        return getLabels result
    }


let emptyTokenFinder: TextDocument.TokenFinder = fun _ _ -> None

let defaultParams =
    CompletionParams(TextDocument = TextDocumentIdentifier(Uri = System.Uri("https://test")), Position = Position())
