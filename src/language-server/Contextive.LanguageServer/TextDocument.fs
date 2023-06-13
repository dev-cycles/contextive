module Contextive.LanguageServer.TextDocument

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open System.Collections.Concurrent
open System.Collections.Generic

open Contextive.LanguageServer.Tokeniser

let private documents = new ConcurrentDictionary<string, IList<string>>()

let private getDocument (documentUri: DocumentUri) =
    let mutable text = null :> IList<string>
    let uriStr = documentUri.ToString()
    match documents.TryGetValue(uriStr, &text) with
    | false -> None
    | true -> Some text

let getTokenAtPosition (lines:IList<string>) (position:Position) =
    Lexer.ofLine lines position.Line
    |> Lexer.getStart position.Character
    |> Lexer.getEnd position.Character
    |> Lexer.get

type TokenFinder = DocumentUri -> Position -> string option

let findToken (documentUri: DocumentUri) (position:Position) =
    match getDocument documentUri with
    | None -> None
    | Some(documentLines) -> getTokenAtPosition documentLines position

let private linesFromText (document:string) : IList<string> = document.ReplaceLineEndings().Split(System.Environment.NewLine)

let private registrationOptionsProvider (hc:SynchronizationCapability) (cc:ClientCapabilities) =
    TextDocumentSyncRegistrationOptions(Change = TextDocumentSyncKind.Full)

let registrationOptions = RegistrationOptionsDelegate<TextDocumentSyncRegistrationOptions, SynchronizationCapability>(registrationOptionsProvider)

module DidOpen = 
    let handler (p:DidOpenTextDocumentParams) =
        let lines = linesFromText p.TextDocument.Text
        let uriStr = p.TextDocument.Uri.ToString()
        documents.AddOrUpdate(uriStr, lines, (fun _ _ -> lines)) |> ignore

module DidChange =
    let handler (p:DidChangeTextDocumentParams) =
        let lines = linesFromText (Seq.head p.ContentChanges).Text
        documents.AddOrUpdate(p.TextDocument.Uri.ToString(), lines, (fun _ _ -> lines)) |> ignore

module DidSave = 
    let handler (p:DidSaveTextDocumentParams) =
        ()

module DidClose = 
    let handler (p:DidCloseTextDocumentParams) =
        documents.TryRemove(p.TextDocument.Uri.ToString()) |> ignore

let getTextDocumentAttributes (uri : DocumentUri) = TextDocumentAttributes(uri, "")

let onSync (b: ILanguageServerRegistry) : ILanguageServerRegistry = 
    b.OnTextDocumentSync(
        TextDocumentSyncKind.Full,
        getTextDocumentAttributes,
        DidOpen.handler,
        DidClose.handler,
        DidChange.handler,
        DidSave.handler,        
        registrationOptions
    )