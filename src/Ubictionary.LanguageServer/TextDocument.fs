module Ubictionary.LanguageServer.TextDocument

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open System.Collections.Concurrent
open System.Collections.Generic

let private documents = new ConcurrentDictionary<string, IList<string>>()

let private getDocument (documentUri: System.Uri) =
    let mutable text = null :> IList<string>
    match documents.TryGetValue(documentUri.ToString(), &text) with
    | false -> None
    | true -> Some text

let private getLines (document:string) : IList<string> = document.Split(System.Environment.NewLine)

type private Word = 
    | Line of string
    | Start of string * int
    | Boundary of string * int * int
    | NoWord

let private getLine (lines:IList<string>) lineNumber =
    if lineNumber >= lines.Count then
        NoWord
    else
        Line(lines[lineNumber])

let private getWordStart (character: int) (line:Word) =
    match line with
    | Line(line) ->
        if character >= line.Length then
            NoWord
        else
            Start(line, line.LastIndexOf(" ", character) + 1)
    | _ -> NoWord

let private getWordEnd (character: int) (line: Word) =
    match line with
    | Start(line, wordStart) ->
        let wordEnd = line.IndexOf(" ", character)
        let length = (if wordEnd < 0 then line.Length else wordEnd) - wordStart
        if length < 0 then
            NoWord
        else
            Boundary(line, wordStart, length)
    | _ -> NoWord
       
let private getWordFromBoundary (line: Word) =
    match line with 
    | Boundary(line, wordStart, length) -> line.Substring(wordStart, length) |> Some
    | _ -> None

let getWordAtPosition (lines:IList<string>) (position:Position) =
    getLine lines position.Line
    |> getWordStart position.Character
    |> getWordEnd position.Character
    |> getWordFromBoundary

let getWord (documentUri: System.Uri) (position:Position) =
    match getDocument documentUri with
    | None -> None
    | Some(document) -> getWordAtPosition document position

module DidOpen = 
    let handler (p:DidOpenTextDocumentParams) =
        let lines = getLines p.TextDocument.Text
        documents.AddOrUpdate(p.TextDocument.Uri.ToString(), lines, (fun _ _ -> lines)) |> ignore

    let private registrationOptionsProvider (hc:SynchronizationCapability) (cc:ClientCapabilities) =
        TextDocumentOpenRegistrationOptions()

    let registrationOptions = RegistrationOptionsDelegate<TextDocumentOpenRegistrationOptions, SynchronizationCapability>(registrationOptionsProvider)