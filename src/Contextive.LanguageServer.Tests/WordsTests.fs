module Contextive.LanguageServer.Tests.WordsTests

open Contextive.LanguageServer
open Expecto
open Swensen.Unquote

[<Tests>]
let definitionsTests =
    testList "Words  Tests" [

        let compareWordAndParts (w1:Words.WordAndParts) (w2:Words.WordAndParts) =
            ((fst w1, fst w2) ||> compare) +
            ((snd w1, snd w2) ||> Seq.compareWith compare)


        let testWordSplitting (word, (expectedWords: Words.WordAndParts seq)) =
            let expectedWordsList = sprintf "%A" <| Seq.toList expectedWords
            testCase $"{word}: splits to {expectedWordsList}" <|
                fun () -> 
                    let words = Some word |> Words.splitIntoWordAndParts
                    test <@ (words, expectedWords) ||> Seq.compareWith compareWordAndParts = 0 @>

        [
            ("firstword",
                seq {
                    ("firstword", seq {"firstword"})
                })
            ("camelCase",
                seq {
                    ("camel", seq {"camel"});
                    ("Case", seq {"Case"});
                    ("camelCase",seq {"camel";"Case"})
                })
            ("PascalCase", 
                seq {
                    ("Pascal", seq {"Pascal"});
                    ("Case", seq {"Case"});
                    ("PascalCase", seq {"Pascal"; "Case"});
                })
            ("snake_case", 
                seq {
                    ("snake", seq {"snake"});
                    ("case", seq {"case"});
                    ("snakecase", seq {"snake"; "case" });
                })
            ("PascalCaseId",
                seq {
                    ("Pascal", seq {"Pascal"});
                    ("Case", seq {"Case"});
                    ("Id", seq{"Id"});
                    ("PascalCase", seq{"Pascal";"Case"});
                    ("CaseId", seq{"Case";"Id";});
                    ("PascalCaseId", seq{"Pascal";"Case";"Id"});
                })
        ]
        |> List.map testWordSplitting |> testList "Wordfinding Tests"


    ]