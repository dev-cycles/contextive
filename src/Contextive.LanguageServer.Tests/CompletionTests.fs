module Contextive.LanguageServer.Tests.CompletionTests

open Expecto
open Swensen.Unquote
open System.IO
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open Contextive.LanguageServer
open Contextive.LanguageServer.Definitions
open TestClient

module DH = Contextive.LanguageServer.Tests.Definitions

[<Tests>]
let completionTests =
    testSequenced <| testList "Completion Tests" [
        testAsync "Given no contextive respond with empty completion list " {
            use! client = SimpleTestClient |> init

            let! labels = Completion.getCompletionLabels client

            test <@ Seq.length labels = 0 @>
        }


        let testFileReader (fileName, text, position, expectedCompletionLabels) =
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

                test <@ (completionLabels, expectedCompletionLabels) ||> Seq.compareWith compare = 0 @>
            }

        [
            ("one", "", Position(0, 0), Fixtures.One.expectedCompletionLabels)
            ("two", "", Position(0, 0), Fixtures.Two.expectedCompletionLabels)
            ("two", "W", Position(0, 1), Fixtures.Two.expectedCompletionLabelsPascal)
            ("two", "WO", Position(0, 2), Fixtures.Two.expectedCompletionLabelsUPPER)
        ] |> List.map testFileReader |> testList "File reading tests"

        let singleWordCompletion (term, (wordAtPosition:string option), expectedLabel:string) = 
            testCase $"Completion of \"{term}\" with {wordAtPosition} at position, returns \"{expectedLabel}\"" <| fun () -> 
                let finder : Definitions.Finder = DH.mockTermsFinder Context.Default ([term])

                let wordGetter : TextDocument.WordGetter = fun _ _ -> wordAtPosition

                let completionLabels =
                    (Completion.handler finder wordGetter Completion.defaultParams null null).Result
                    |> Completion.getLabels

                test <@ (completionLabels, seq {expectedLabel}) ||> Seq.compareWith compare = 0 @>
        [
            ("term", Some "", "term")
            ("Term", Some "", "Term")
            ("term", Some "t", "term")
            ("Term", Some "t", "term")
            ("term", Some "T", "Term")
            ("term", Some "Te", "Term")
            ("term", Some "TE","TERM")
            ("term", Some "TEr","Term")
            ("term", None, "term")
        ] |> List.map singleWordCompletion |> testList "Single Word Completion"


        let multiWordCompletion (term, (wordAtPosition:string option), expectedCompletionLabels:string seq) = 
            let expectedCompletionLabelsList = sprintf "%A" <| Seq.toList expectedCompletionLabels
            testCase $"Completion of \"{term}\" with {wordAtPosition} at position, returns \"{expectedCompletionLabelsList}\"" <| fun () -> 
                let finder : Definitions.Finder = DH.mockTermsFinder Context.Default ([term])

                let wordGetter : TextDocument.WordGetter = fun _ _ -> wordAtPosition

                let completionLabels =
                    (Completion.handler finder wordGetter Completion.defaultParams null null).Result
                    |> Completion.getLabels

                test <@ (completionLabels, expectedCompletionLabels) ||> Seq.compareWith compare = 0 @>
        [
            ("Multi Word", Some "", seq {"multiWord";"MultiWord";"multi_word"})
            ("Multi Word", Some "m", seq {"multiWord";"multi_word"})
            ("Multi Word", Some "M", seq {"MultiWord";"MULTI_WORD"})
            ("Multi Word", Some "MU", seq {"MULTIWORD";"MULTI_WORD"})
            ("multi word", Some "", seq {"multiWord";"MultiWord";"multi_word"})
            ("multi word", Some "m", seq {"multiWord";"multi_word"})
            ("multi word", Some "M", seq {"MultiWord";"MULTI_WORD"})
            ("multi word", Some "MU", seq {"MULTIWORD";"MULTI_WORD"})
        ] |> List.map multiWordCompletion |> testList "Multi Word Completion"

        let detailCompletion (contextName, expectedDetail) = 
            testCase $"Context \"{contextName}\" has detail \"{expectedDetail}\"" <| fun () ->
                let finder : Definitions.Finder = DH.mockTermsFinder {Context.Default with Name=contextName} (["term"])

                let completionLabels =
                    (Completion.handler finder Completion.emptyWordGetter Completion.defaultParams null null).Result

                test <@ (completionLabels |> Seq.head).Detail = expectedDetail @>
        [
            (null, null)
            ("context name", "context name Context")
        ] |> List.map detailCompletion |> testList "Detail Completion"
    ]