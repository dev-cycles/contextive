module Contextive.LanguageServer.Tokeniser

open System.Collections.Generic
open System.Text.RegularExpressions
open System.Linq

let private (|EmptySeq|_|) a = if Seq.isEmpty a then Some() else None

let private (|Regex|_|) pattern input =
    let res =
        Regex.Matches(input, pattern).OfType<Match>().Select(fun m -> m.Value)
        |> Seq.cast<string>

    match res with
    | EmptySeq -> None
    | _ -> res |> Some

let private adjustCharacterForTrailingSpace character = System.Math.Max(0, character - 1)

/// Chars that shouldn't be delimiters (as they might be in the middle of a token), but should be trimmed if at start or end of a token
let private trimChars = [| ' '; '-' |]

let private trim (token:string) : string = 
    token.Trim(trimChars)

type Lexer =
    | Line of line: string
    | Start of line: string * start: int
    | Token of line: string * start: int * end': int
    | NoToken

    member this.Length =
        match this with
        | Token(line, start, end') -> end' - start |> Some
        | _ -> None

    member this.HasLength =
        match this.Length with
        | Some(length) -> length > 0
        | _ -> false

    static member private delimiters =
        [| ' '; '('; '.'; '>'; ':'; ','; ')'; '{'; '}'; '['; ']' |]

    static member private startOfToken line position =
        Start(line, line.LastIndexOfAny(Lexer.delimiters, position) + 1)

    static member ofLine(lines: IList<string>) =
        function
        | lineNumber when lineNumber >= lines.Count -> NoToken
        | lineNumber -> Line(lines[lineNumber])

    static member getStart(character: int) =
        function
        | Line(line) when character < line.Length ->
            match line[character] with
            | ' ' -> adjustCharacterForTrailingSpace character |> Lexer.startOfToken line
            | _ -> character |> Lexer.startOfToken line
        | Line(line) when character = line.Length ->
            adjustCharacterForTrailingSpace character |> Lexer.startOfToken line
        | _ -> NoToken

    static member getEnd(character: int) =
        function
        | Start(line, start) ->
            let end' = line.IndexOfAny(Lexer.delimiters, character)
            let end' = if end' < 0 then line.Length else end'
            Token(line, start, end')
        | _ -> NoToken

    static member get =
        function
        | Token(line, start, _) as t when t.HasLength -> line.Substring(start, t.Length.Value) |> trim |> Some
        | _ -> None
