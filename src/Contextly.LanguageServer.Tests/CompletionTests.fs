module Contextly.LanguageServer.Tests.CompletionTests

open Expecto
open Swensen.Unquote
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Document
open Contextly.LanguageServer
open Contextly.LanguageServer.Definitions
open TestClient

[<Tests>]
let completionTests =
    testSequenced <| testList "Completion Tests" [
        testAsync "Given no contextly respond with empty completion list " {
            use! client = SimpleTestClient |> init

            let completionParams = CompletionParams()

            let! result = client.TextDocument.RequestCompletion(completionParams).AsTask() |> Async.AwaitTask
        
            test <@ Seq.length result.Items = 0 @>
        }

        let testFileReader (fileName, text, position, expectedList) =
            testAsync $"Given {fileName} contextly, in document {text} at position {position} respond with expected completion list " {
                let config = [
                    Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.contextlyPathOptionsBuilder $"{fileName}.yml"
                ]

                use! client = TestClient(config) |> init
            
                let textDocumentUri = $"file:///{System.Guid.NewGuid().ToString()}"

                client.TextDocument.DidOpenTextDocument(DidOpenTextDocumentParams(TextDocument = TextDocumentItem(
                    LanguageId = "plaintext",
                    Version = 0,
                    Text = text,
                    Uri = textDocumentUri
                )))

                let completionParams = CompletionParams(
                    TextDocument = textDocumentUri,
                    Position = position
                )

                let! result = client.TextDocument.RequestCompletion(completionParams).AsTask() |> Async.AwaitTask

                test <@ result.IsIncomplete @>

                let completionLabels = result.Items |> Seq.map (fun x -> x.Label)

                test <@ (completionLabels, expectedList) ||> Seq.compareWith compare = 0 @>
            }

        [
            ("one", "", Position(0, 0), ["firstTerm";"secondTerm";"thirdTerm"])
            ("two", "", Position(0, 0), ["word1";"word2";"word3"])
            ("two", "W", Position(0, 1), ["Word1";"Word2";"Word3"])
            ("two", "WO", Position(0, 2), ["WORD1";"WORD2";"WORD3"])
        ] |> List.map testFileReader |> testList "File reading tests"

        let completionCaseMatching (term, wordAtPosition, expectedCompletionLabel:string) = 
            testCase $"Completion of \"{term}\" with {wordAtPosition} at position, returns \"{expectedCompletionLabel}\"" <| fun () -> 
                let finder : Definitions.Finder = fun _ -> seq { {Term.Default with Name = term} } 

                let wordGetter : TextDocument.WordGetter = fun _ _ -> wordAtPosition

                let completionParams = CompletionParams(
                    TextDocument = TextDocumentIdentifier(Uri = new System.Uri("https://test")),
                    Position = Position()
                )

                let result = (Completion.handler finder wordGetter completionParams null null).Result

                let completionLabels = result.Items |> Seq.map (fun x -> x.Label)

                test <@ (completionLabels, seq {expectedCompletionLabel}) ||> Seq.compareWith compare = 0 @>
        [
            ("term", Some(""), "term")
            ("Term", Some(""), "Term")
            ("term", Some("t"), "term")
            ("Term", Some("t"), "term")
            ("term", Some("T"), "Term")
            ("term", Some("Te"), "Term")
            ("term", Some("TE"),"TERM")
            ("term", Some("TEr"),"Term")
            ("term", None, "term")
        ] |> List.map completionCaseMatching |> testList "Completion Case Matching"
    ]