module Contextive.LanguageServer.Tests.Component.TextDocumentTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities
open System.IO
open Contextive.LanguageServer.Tests.Helpers
open Tests.Helpers.TestClient

[<Tests>]
let tests =
    testList
        "LanguageServer.TextDocument Tests"
        [

          let testTokenFinding (name, lines, position, expectedWord: string option) =
              testCase $"{name}: finds {expectedWord} at position {position}"
              <| fun () ->
                  let lines = ResizeArray<string>(seq lines)
                  let token = TextDocument.getTokenAtPosition lines position
                  test <@ token = expectedWord @>

          [ "single word", [ "firstword"; "secondword" ], Position(0, 0), Some "firstword"
            "single word", [ "firstword"; "secondword" ], Position(1, 0), Some "secondword"
            "multiple words", [ "firstword secondword" ], Position(0, 0), Some "firstword"
            "multiple words", [ "firstword secondword" ], Position(0, 10), Some "secondword"
            "multiple words", [ "firstword secondword" ], Position(0, 15), Some "secondword"
            "position at end", [ "firstword secondword" ], Position(0, 20), Some "secondword"
            "method", [ "firstword()" ], Position(0, 1), Some "firstword"
            "object", [ "firstword.method()" ], Position(0, 1), Some "firstword"
            "object arrow", [ "firstword->method()" ], Position(0, 1), Some "firstword"
            "object property", [ "firstWord.property" ], Position(0, 10), Some "property"
            "object arrow prop", [ "firstWord->property" ], Position(0, 11), Some "property"
            "object prop clause", [ "firstWord.property " ], Position(0, 18), Some "property"
            "object method", [ "firstWord.method()" ], Position(0, 10), Some "method"
            "array", [ "[firstelement]" ], Position(0, 10), Some "firstelement"
            "array", [ "[firstElement,secondelement]" ], Position(0, 15), Some "secondelement"
            "argument", [ "(firstelement)" ], Position(0, 10), Some "firstelement"
            "braces", [ "{firstelement}" ], Position(0, 10), Some "firstelement"
            "yaml key", [ "key: value" ], Position(0, 1), Some "key"
            "sentence", [ "word, something" ], Position(0, 1), Some "word"
            "position at end", [ "firstWord secondWord" ], Position(0, 21), None
            "position on space", [ "firstword secondWord" ], Position(0, 9), Some "firstword"
            "out of range line", [ "firstWord secondWord" ], Position(1, 0), None
            "out of range char", [ "firstWord" ], Position(0, 50), None
            "snake_case", [ "snake_case()" ], Position(0, 1), Some "snake_case"
            "kebab-case", [ "kebab-case:" ], Position(0, 1), Some "kebab-case" ]
          |> List.map testTokenFinding
          |> testList "Wordfinding Tests"

          testAsync "Server supports full sync" {
              let config =
                  [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.contextivePathBuilder "one.yml" ]

              use! client = TestClient(config) |> init
              test <@ client.ServerSettings.Capabilities.TextDocumentSync.Options.Change = TextDocumentSyncKind.Full @>
          }

          let openDocumentCanFindTextDoc (name: string, uri: string) =
              testAsync name {
                  let textDocumentUri = DocumentUri.From(uri)

                  TextDocument.DidOpen.handler (
                      DidOpenTextDocumentParams(
                          TextDocument =
                              TextDocumentItem(
                                  LanguageId = "plaintext",
                                  Version = 0,
                                  Text = "firstterm",
                                  Uri = textDocumentUri
                              )
                      )
                  )

                  let token = TextDocument.findToken textDocumentUri <| Position(0, 0)

                  test <@ token.Value = "firstterm" @>
              }

          [ "simple", $"file:///{System.Guid.NewGuid().ToString()}"
            "complex", $"file:///{System.Guid.NewGuid().ToString()}/Some- complex path&/file.txt" ]
          |> List.map openDocumentCanFindTextDoc
          |> testList "Given open text document, can find text document"

          testAsync "Given changed text document, can find text document" {
              let textDocumentUri = DocumentUri.From $"file:///{System.Guid.NewGuid().ToString()}"

              TextDocument.DidOpen.handler (
                  DidOpenTextDocumentParams(
                      TextDocument =
                          TextDocumentItem(LanguageId = "plaintext", Text = "firstTerm", Uri = textDocumentUri)
                  )
              )

              TextDocument.DidChange.handler (
                  DidChangeTextDocumentParams(
                      TextDocument = OptionalVersionedTextDocumentIdentifier(Uri = textDocumentUri),
                      ContentChanges = Container(TextDocumentContentChangeEvent(Text = "secondterm"))
                  )
              )

              let token = TextDocument.findToken textDocumentUri <| Position(0, 0)

              test <@ token.Value = "secondterm" @>
          }

          testAsync "Given Saved document, can find text document" {
              let textDocumentUri =
                  DocumentUri.From($"file:///{System.Guid.NewGuid().ToString()}")

              TextDocument.DidOpen.handler (
                  DidOpenTextDocumentParams(
                      TextDocument =
                          TextDocumentItem(LanguageId = "plaintext", Text = "firstTerm", Uri = textDocumentUri)
                  )
              )

              TextDocument.DidChange.handler (
                  DidChangeTextDocumentParams(
                      TextDocument = OptionalVersionedTextDocumentIdentifier(Uri = textDocumentUri),
                      ContentChanges = Container(TextDocumentContentChangeEvent(Text = "secondterm"))
                  )
              )

              TextDocument.DidSave.handler (
                  DidSaveTextDocumentParams(
                      TextDocument = OptionalVersionedTextDocumentIdentifier(Uri = textDocumentUri)
                  )
              )

              let token = TextDocument.findToken textDocumentUri <| Position(0, 0)

              test <@ token.Value = "secondterm" @>
          }

          testAsync "Given Closed document, text document not found" {
              let textDocumentUri =
                  DocumentUri.From($"file:///{System.Guid.NewGuid().ToString()}")

              TextDocument.DidOpen.handler (
                  DidOpenTextDocumentParams(
                      TextDocument =
                          TextDocumentItem(LanguageId = "plaintext", Text = "firstTerm", Uri = textDocumentUri)
                  )
              )

              TextDocument.DidClose.handler (
                  DidCloseTextDocumentParams(
                      TextDocument = OptionalVersionedTextDocumentIdentifier(Uri = textDocumentUri)
                  )
              )

              let token = TextDocument.findToken textDocumentUri <| Position(0, 0)

              test <@ token.IsNone @>
          } ]
