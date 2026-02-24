module Contextive.Core.Tests.CandidateTermsTests

open Contextive.Core.CandidateTerms
open Expecto
open Swensen.Unquote

[<Tests>]
let tokenAndPartsTests =
    testList
        "Core.Candidate Terms Tests"
        [

          let compareTokenAndCandidateTerms (t1: TokenAndCandidateTerms) (t2: TokenAndCandidateTerms) =
              ((fst t1, fst t2) ||> compare) + ((snd t1, snd t2) ||> Seq.compareWith compare)


          let testCandidateTermFinding (token, expectedCandidateTerms: TokenAndCandidateTerms seq) =
              let expectedCandidateTermsList =
                  sprintf "%A" <| Seq.toList expectedCandidateTerms
                  |> _.Replace(System.Environment.NewLine, " ")

              testCase $"Token {token}: finds candidates {expectedCandidateTermsList}"
              <| fun () ->
                  let tokenAndCandidateTerms = Some token |> tokenToTokenAndCandidateTerms

                  test
                      <@
                          (tokenAndCandidateTerms, expectedCandidateTerms)
                          ||> Seq.compareWith compareTokenAndCandidateTerms = 0
                      @>

          testList
              "CJK Detection Tests"
              [ testCase "English text is not CJK"
                <| fun () -> test <@ containsCJK "reverseAuction" = false @>

                testCase "Japanese kanji is CJK"
                <| fun () -> test <@ containsCJK "注文が届く" = true @>

                testCase "Japanese katakana is CJK"
                <| fun () -> test <@ containsCJK "カテゴリ" = true @>

                testCase "Chinese text is CJK"
                <| fun () -> test <@ containsCJK "购物车" = true @>

                testCase "Korean text is CJK"
                <| fun () -> test <@ containsCJK "사용자" = true @>

                testCase "Mixed English and Japanese is CJK"
                <| fun () -> test <@ containsCJK "Orderマッピング" = true @>

                testCase "Empty string is not CJK"
                <| fun () -> test <@ containsCJK "" = false @>

                testCase "Cyrillic is not CJK"
                <| fun () -> test <@ containsCJK "ОдинДва" = false @> ]

          [ ("firstword", seq { ("firstword", seq { "firstword" }) })
            ("camelCase",
             seq {
                 ("camel", seq { "camel" })
                 ("Case", seq { "Case" })

                 ("camelCase",
                  seq {
                      "camel"
                      "Case"
                  })
             })
            ("PascalCase",
             seq {
                 ("Pascal", seq { "Pascal" })
                 ("Case", seq { "Case" })

                 ("PascalCase",
                  seq {
                      "Pascal"
                      "Case"
                  })
             })
            ("snake_case",
             seq {
                 ("snake", seq { "snake" })
                 ("case", seq { "case" })

                 ("snakecase",
                  seq {
                      "snake"
                      "case"
                  })
             })
            ("kebab-case",
             seq {
                 ("kebab", seq { "kebab" })
                 ("case", seq { "case" })

                 ("kebabcase",
                  seq {
                      "kebab"
                      "case"
                  })
             })
            ("PascalCaseId",
             seq {
                 ("Pascal", seq { "Pascal" })
                 ("Case", seq { "Case" })
                 ("Id", seq { "Id" })

                 ("PascalCase",
                  seq {
                      "Pascal"
                      "Case"
                  })

                 ("CaseId",
                  seq {
                      "Case"
                      "Id"
                  })

                 ("PascalCaseId",
                  seq {
                      "Pascal"
                      "Case"
                      "Id"
                  })
             }) ]
          |> List.map testCandidateTermFinding
          |> testList "Candidate Term Finding Tests"

          ]
