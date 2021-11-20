module Ubictionary.LanguageServer.Tests.TextDocumentTests

open Expecto
open Swensen.Unquote
open Ubictionary.LanguageServer
open OmniSharp.Extensions.LanguageServer.Protocol.Models

[<Tests>]
let textDocumentTests =
    testList "TextDocument Tests" [

        let testWordFinding (name, lines, position, (expectedWord: string option)) =
            ftestCase $"{name}: Can identify {expectedWord} at position {position}" <|
                fun () -> 
                    let lines = ResizeArray<string>(seq lines)
                    let word = TextDocument.getWordAtPosition lines <| position
                    test <@ word = expectedWord @>

        [
            ("single word", ["firstWord"; "secondWord"], Position(0,0), Some "firstWord")
            ("single word", ["firstWord"; "secondWord"], Position(1,0), Some "secondWord")
            ("multiple words", ["firstWord secondWord"], Position(0,0), Some "firstWord")
            ("multiple words", ["firstWord secondWord"], Position(0,10), Some "secondWord")
            ("multiple words", ["firstWord secondWord"], Position(0,15), Some "secondWord")
            ("position at end", ["firstWord secondWord"], Position(0,20), None)
            ("position on space", ["firstWord secondWord"], Position(0,9), None)
            ("out of range lines", ["firstWord secondWord"], Position(1,0), None)
            ("out of range character", ["firstWord"], Position(0,50), None)
        ]
        |> List.map testWordFinding |> testList "Wordfinding Tests"
        
    ]