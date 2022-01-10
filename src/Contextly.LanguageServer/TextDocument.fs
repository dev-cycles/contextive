module Contextly.LanguageServer.TextDocument

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open System.Collections.Concurrent
open System.Collections.Generic

let private documents = new ConcurrentDictionary<string, IList<string>>()

let private getDocument (documentUri: System.Uri) =
    let mutable text = null :> IList<string>
    match documents.TryGetValue(documentUri.ToString(), &text) with
    | false -> None
    | true -> Some text

type private Word = 
    | Line of line: string
    | Start of line: string * start: int
    | Token of line: string * start: int * end' : int
    | NoWord
    member this.Length = 
        match this with | Token(line, start, end') -> end' - start |> Some
                        | _ -> None
    member this.HasLength = 
        match this.Length with | Some(length) -> length > 0
                               | _ -> false

    static member private delimiters = [|' ';'(';'.';'-';'>';':';',';')';'{';'}';'[';']'|]

    static member private startOfWord line position = Start(line, line.LastIndexOfAny(Word.delimiters, position) + 1)

    static member ofLine (lines:IList<string>) =
        function | lineNumber when lineNumber >= lines.Count -> NoWord
                 | lineNumber -> Line(lines[lineNumber])

    static member getStart (character: int) =
        function | Line(line) when character < line.Length -> 
                    match line[character] with
                    | ' ' -> Word.startOfWord line <| character-1
                    | _ -> Word.startOfWord line character
                 | Line(line) when character = line.Length -> 
                    Word.startOfWord line <| character-1
                 | _ -> NoWord

    static member getEnd (character: int) =
        function | Start(line, start) -> 
                    let end' = line.IndexOfAny(Word.delimiters, character)
                    let end' = if end' < 0 then line.Length else end'
                    Token(line, start, end')
                 | _ -> NoWord
       
    static member get =
        function | Token(line, start, _) as t when t.HasLength -> 
                    line.Substring(start, t.Length.Value) |> Some
                 | _ -> None

let getWordAtPosition (lines:IList<string>) (position:Position) =
    Word.ofLine lines position.Line
    |> Word.getStart position.Character
    |> Word.getEnd position.Character
    |> Word.get

type WordGetter = System.Uri -> Position -> string option

let getWord (documentUri: System.Uri) (position:Position) =
    match getDocument documentUri with
    | None -> None
    | Some(document) -> getWordAtPosition document position

let private getLines (document:string) : IList<string> = document.Split(System.Environment.NewLine)

let private registrationOptionsProvider (hc:SynchronizationCapability) (cc:ClientCapabilities) =
    TextDocumentSyncRegistrationOptions(Change = TextDocumentSyncKind.Full)

let registrationOptions = RegistrationOptionsDelegate<TextDocumentSyncRegistrationOptions, SynchronizationCapability>(registrationOptionsProvider)

module DidOpen = 
    let handler (p:DidOpenTextDocumentParams) =
        let lines = getLines p.TextDocument.Text
        documents.AddOrUpdate(p.TextDocument.Uri.ToString(), lines, (fun _ _ -> lines)) |> ignore

module DidChange =
    let handler (p:DidChangeTextDocumentParams) =
        let lines = getLines (Seq.head p.ContentChanges).Text
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