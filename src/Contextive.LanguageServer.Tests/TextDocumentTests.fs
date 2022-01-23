module Contextive.LanguageServer.Tests.TextDocumentTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities
open TestClient
open System.IO

[<Tests>]
let textDocumentTests =
    testList "TextDocument Tests" [

        let testWordFinding (name, lines, position, (expectedWord: string option)) =
            testCase $"{name}: Can identify {expectedWord} at position {position}" <|
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
            ("position at end", ["firstWord secondWord"], Position(0,20), Some "secondWord")
            ("method", ["firstWord()"], Position(0,1), Some "firstWord")
            ("object", ["firstWord.method()"], Position(0,1), Some "firstWord")
            ("object arrow", ["firstWord->method()"], Position(0,1), Some "firstWord")
            ("object property", ["firstWord.property"], Position(0,10), Some "property")
            ("object arrow property", ["firstWord->property"], Position(0,11), Some "property")
            ("object property in clause", ["firstWord.property "], Position(0,18), Some "property")
            ("object method", ["firstWord.method()"], Position(0,10), Some "method")
            ("array", ["[firstElement]"], Position(0,10), Some "firstElement")
            ("array", ["[firstElement,secondElement]"], Position(0,15), Some "secondElement")
            ("argument", ["(firstElement)"], Position(0,10), Some "firstElement")
            ("braces", ["{firstElement}"], Position(0,10), Some "firstElement")
            ("yaml key", ["key: value"], Position(0,1), Some "key")
            ("sentence", ["word, something"], Position(0,1), Some "word")
            ("position at end", ["firstWord secondWord"], Position(0,21), None)
            ("position on space", ["firstWord secondWord"], Position(0,9), Some "firstWord")
            ("out of range lines", ["firstWord secondWord"], Position(1,0), None)
            ("out of range character", ["firstWord"], Position(0,50), None)
        ]
        |> List.map testWordFinding |> testList "Wordfinding Tests"
        
        testAsync "Server supports full sync" {
            let config = [
                        Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                        ConfigurationSection.contextivePathOptionsBuilder $"one.yml"
                    ]

            use! client = TestClient(config) |> init
            test <@ client.ServerSettings.Capabilities.TextDocumentSync.Options.Change = TextDocumentSyncKind.Full @>
        }

        testAsync $"Given open text document, can find text document" {
            let textDocumentUri = System.Uri($"file:///{System.Guid.NewGuid().ToString()}")

            TextDocument.DidOpen.handler(DidOpenTextDocumentParams(TextDocument = TextDocumentItem(
                LanguageId = "plaintext",
                Version = 0,
                Text = "firstTerm",
                Uri = textDocumentUri
            )))

            let word = TextDocument.getWord textDocumentUri <| Position(0, 0)

            test <@ word.IsSome @>
            test <@ word.Value = "firstTerm" @>
        }
        
        testAsync $"Given changed text document, can find text document" {
            let textDocumentUri = System.Uri($"file:///{System.Guid.NewGuid().ToString()}")

            TextDocument.DidOpen.handler(DidOpenTextDocumentParams(TextDocument = TextDocumentItem(
                LanguageId = "plaintext",
                Text = "firstTerm",
                Uri = textDocumentUri
            )))

            TextDocument.DidChange.handler(DidChangeTextDocumentParams(
                TextDocument = OptionalVersionedTextDocumentIdentifier(Uri = textDocumentUri),
                ContentChanges = Container(TextDocumentContentChangeEvent(Text = "secondTerm"))
            ))

            let word = TextDocument.getWord textDocumentUri <| Position(0, 0)

            test <@ word.IsSome @>
            test <@ word.Value = "secondTerm" @>
        }

        testAsync $"Given Saved document, can find text document" {
            let textDocumentUri = System.Uri($"file:///{System.Guid.NewGuid().ToString()}")

            TextDocument.DidOpen.handler(DidOpenTextDocumentParams(TextDocument = TextDocumentItem(
                LanguageId = "plaintext",
                Text = "firstTerm",
                Uri = textDocumentUri
            )))

            TextDocument.DidChange.handler(DidChangeTextDocumentParams(
                TextDocument = OptionalVersionedTextDocumentIdentifier(Uri = textDocumentUri),
                ContentChanges = Container(TextDocumentContentChangeEvent(Text = "secondTerm"))
            ))

            TextDocument.DidSave.handler(DidSaveTextDocumentParams(
                TextDocument = OptionalVersionedTextDocumentIdentifier(Uri = textDocumentUri)
            ))

            let word = TextDocument.getWord textDocumentUri <| Position(0, 0)

            test <@ word.IsSome @>
            test <@ word.Value = "secondTerm" @>
        }

        testAsync $"Given Closed document, text document not found" {
            let textDocumentUri = System.Uri($"file:///{System.Guid.NewGuid().ToString()}")

            TextDocument.DidOpen.handler(DidOpenTextDocumentParams(TextDocument = TextDocumentItem(
                LanguageId = "plaintext",
                Text = "firstTerm",
                Uri = textDocumentUri
            )))

            TextDocument.DidClose.handler(DidCloseTextDocumentParams(
                TextDocument = OptionalVersionedTextDocumentIdentifier(Uri = textDocumentUri)
            ))

            let word = TextDocument.getWord textDocumentUri <| Position(0, 0)

            test <@ word.IsNone @>
        }

    ]