module Contextive.LanguageServer.TextDocument

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities
open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open System.Collections.Concurrent
open System.Collections.Generic
open System.Text.RegularExpressions
open System.Linq

let (|EmptySeq|_|) a = if Seq.isEmpty a then Some () else None

let (|Regex|_|) pattern input =
    let res = Regex.Matches(input, pattern)
                .OfType<Match>()
                .Select(fun m -> m.Value)
                |> Seq.cast<string>
    match res with
    | EmptySeq -> None
    | _ -> res |> Some

let private documents = new ConcurrentDictionary<string, IList<string>>()

let private getDocument (documentUri: System.Uri) =
    let mutable text = null :> IList<string>
    match documents.TryGetValue(documentUri.ToString(), &text) with
    | false -> None
    | true -> Some text

type private Words = 
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
    static member private wordSplitterRegex = "([A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9]+|[a-z]+)"

    static member Default = Seq.empty<string>

    static member private startOfWord line position = Start(line, line.LastIndexOfAny(Words.delimiters, position) + 1)

    static member ofLine (lines:IList<string>) =
        function | lineNumber when lineNumber >= lines.Count -> NoWord
                 | lineNumber -> Line(lines[lineNumber])

    static member getStart (character: int) =
        function | Line(line) when character < line.Length -> 
                    match line[character] with
                    | ' ' -> Words.startOfWord line <| character-1
                    | _ -> Words.startOfWord line character
                 | Line(line) when character = line.Length -> 
                    Words.startOfWord line <| character-1
                 | _ -> NoWord

    static member getEnd (character: int) =
        function | Start(line, start) -> 
                    let end' = line.IndexOfAny(Words.delimiters, character)
                    let end' = if end' < 0 then line.Length else end'
                    Token(line, start, end')
                 | _ -> NoWord

    static member getWord =
        function | Token(line, start, _) as t when t.HasLength -> 
                    line.Substring(start, t.Length.Value) |> Some
                 | _ -> None

    static member split =
        function | None -> Words.Default
                 | Some(Regex Words.wordSplitterRegex words) -> words
                 | Some(w) -> seq {w}

    static member combine wordList =
        seq { 1 .. (Seq.length wordList) }
        |> Seq.collect (fun chunkSize -> 
            Seq.windowed chunkSize wordList
            |> Seq.map (fun chunk -> (String.concat "" chunk, Seq.cast<string> chunk) ))
    
    static member get = Words.getWord >> Words.split >> Words.combine >> List.ofSeq

let getWordAtPosition (lines:IList<string>) (position:Position) =
    Words.ofLine lines position.Line
    |> Words.getStart position.Character
    |> Words.getEnd position.Character
    |> Words.get

type WordAndParts = string * seq<string>
type WordGetter = System.Uri -> Position -> WordAndParts list

let getWords (documentUri: System.Uri) (position:Position) =
    match getDocument documentUri with
    | None -> []
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