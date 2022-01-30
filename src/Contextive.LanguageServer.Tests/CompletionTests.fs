module Contextive.LanguageServer.Tests.CompletionTests

open Expecto
open Swensen.Unquote
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open Contextive.LanguageServer
open Contextive.LanguageServer.Definitions
open TestClient

[<Tests>]
let completionTests =
    testSequenced <| testList "Completion Tests" [
        testAsync "Given no contextive respond with empty completion list " {
            use! client = SimpleTestClient |> init

            let! completionLabels = Completion.getCompletionLabels client
        
            test <@ Seq.length completionLabels = 0 @>
        }

        let testFileReader (fileName, text, position, expectedList) =
            testAsync $"Given {fileName} contextive, in document {text} at position {position} respond with expected completion list " {
                let config = [
                    Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.contextivePathOptionsBuilder $"{fileName}.yml"
                ]

                use! client = TestClient(config) |> init
            
                let textDocumentUri = $"file:///{System.Guid.NewGuid().ToString()}"

                let! result = client |> Completion.getCompletionFromText text textDocumentUri position

                test <@ result.IsIncomplete @>

                let completionLabels = Completion.getLabels result

                test <@ (completionLabels, expectedList) ||> Seq.compareWith compare = 0 @>
            }

        [
            ("one", "", Position(0, 0), ["firstTerm";"secondTerm";"thirdTerm"])
            ("two", "", Position(0, 0), ["word1";"word2";"word3"])
            ("two", "W", Position(0, 1), ["Word1";"Word2";"Word3"])
            ("two", "WO", Position(0, 2), ["WORD1";"WORD2";"WORD3"])
        ] |> List.map testFileReader |> testList "File reading tests"

        let quoter = fun s -> $"\"{s}\""

        let completionCaseMatching (term, wordsAtPosition, expectedCompletionLabel:string) = 
            testCase $"Completion of \"{term}\" with {wordsAtPosition |> List.map quoter} at position, returns \"{expectedCompletionLabel}\"" <| fun () -> 
                let finder : Definitions.Finder = fun _ _ -> async { return seq { {Term.Default with Name = term} } }

                let wordGetter : TextDocument.WordGetter = fun _ _ -> wordsAtPosition

                let completionParams = CompletionParams(
                    TextDocument = TextDocumentIdentifier(Uri = new System.Uri("https://test")),
                    Position = Position()
                )

                let completionLabels =
                    (Completion.handler finder wordGetter completionParams null null).Result
                    |> Completion.getLabels

                test <@ (completionLabels, seq {expectedCompletionLabel}) ||> Seq.compareWith compare = 0 @>
        [
            ("term", [""], "term")
            ("Term", [""], "Term")
            ("term", ["t"], "term")
            ("Term", ["t"], "term")
            ("term", ["T"], "Term")
            ("term", ["Te"], "Term")
            ("term", ["TE"],"TERM")
            ("term", ["TEr"],"Term")
            ("term", [], "term")
        ] |> List.map completionCaseMatching |> testList "Completion Case Matching"
    ]