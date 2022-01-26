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

        let testWordFinding (name, lines, position, (expectedWords: string list)) =
            testCase $"{name}: Can identify {expectedWords} at position {position}" <|
                fun () -> 
                    let lines = ResizeArray<string>(seq lines)
                    let words = TextDocument.getWordAtPosition lines <| position
                    test <@ words = expectedWords @>

        [
            ("single word", ["firstword"; "secondword"], Position(0,0),  ["firstword"])
            ("single word", ["firstword"; "secondword"], Position(1,0), ["secondword"])
            ("multiple words", ["firstword secondword"], Position(0,0), ["firstword"])
            ("multiple words", ["firstword secondword"], Position(0,10), ["secondword"])
            ("multiple words", ["firstword secondword"], Position(0,15), ["secondword"])
            ("position at end", ["firstword secondword"], Position(0,20), ["secondword"])
            ("method", ["firstword()"], Position(0,1), ["firstword"])
            ("object", ["firstword.method()"], Position(0,1), ["firstword"])
            ("object arrow", ["firstword->method()"], Position(0,1), ["firstword"])
            ("object property", ["firstWord.property"], Position(0,10), ["property"])
            ("object arrow property", ["firstWord->property"], Position(0,11), ["property"])
            ("object property in clause", ["firstWord.property "], Position(0,18), ["property"])
            ("object method", ["firstWord.method()"], Position(0,10), ["method"])
            ("array", ["[firstelement]"], Position(0,10), ["firstelement"])
            ("array", ["[firstElement,secondelement]"], Position(0,15), ["secondelement"])
            ("argument", ["(firstelement)"], Position(0,10), ["firstelement"])
            ("braces", ["{firstelement}"], Position(0,10), ["firstelement"])
            ("yaml key", ["key: value"], Position(0,1), ["key"])
            ("sentence", ["word, something"], Position(0,1), ["word"])
            ("position at end", ["firstWord secondWord"], Position(0,21), [])
            ("position on space", ["firstword secondWord"], Position(0,9), ["firstword"])
            ("out of range lines", ["firstWord secondWord"], Position(1,0), [])
            ("out of range character", ["firstWord"], Position(0,50), [])
            ("camelCase (firstWord)", ["camelCase"], Position(0,3), ["camelCase";"camel";"Case"])
            ("camelCase2 (firstWord)", ["firstWord"], Position(0,6), ["firstWord";"first";"Word"])
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
                Text = "firstterm",
                Uri = textDocumentUri
            )))

            let words = TextDocument.getWords textDocumentUri <| Position(0, 0)

            test <@ words = ["firstterm"] @>
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
                ContentChanges = Container(TextDocumentContentChangeEvent(Text = "secondterm"))
            ))

            let words = TextDocument.getWords textDocumentUri <| Position(0, 0)

            test <@ words = ["secondterm"] @>
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
                ContentChanges = Container(TextDocumentContentChangeEvent(Text = "secondterm"))
            ))

            TextDocument.DidSave.handler(DidSaveTextDocumentParams(
                TextDocument = OptionalVersionedTextDocumentIdentifier(Uri = textDocumentUri)
            ))

            let words = TextDocument.getWords textDocumentUri <| Position(0, 0)

            test <@ words = ["secondterm"] @>
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

            let words = TextDocument.getWords textDocumentUri <| Position(0, 0)

            test <@ words = [] @>
        }

    ]
