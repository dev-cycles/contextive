module Contextive.Core.Tests.NormalizationTests

open Contextive.Core.Normalization
open Expecto
open Swensen.Unquote

[<Tests>]
let normalizationTests =
    testList
        "Core.Normalization Tests"
        [

          let testSimpleNormalization (termName: string, expectedNormalized: string, name: string) =
              testCase $"{name} - Normalizing '{termName}' produces '{expectedNormalized}'"
              <| fun () ->
                  let normalized = simpleNormalize termName

                  test <@ normalized.Equals(expectedNormalized, System.StringComparison.InvariantCulture) @>

          [ "Term", "term", "Simple"
            "Terms", "term", "Plural Simple"
            "Octopi", "octopus", "Plural Complex"
            "Multi word", "multiword", "Two words"
            "Multi words", "multiword", "Two words plural"
            "Multi word octopi", "multiwordoctopus", "Two words plural complex"
            "Many words here", "manywordshere", "Two words"
            "snake_case", "snakecase", "Snake Case"
            "kebab-case", "kebabcase", "Kebab Case"
            "Père", "père", "Diacritic not removed"
            "Auslöser", "auslöser", "Umlaut not removed"
            "Straße", "straße", "ß not replaced"
            "STRAẞE", "straße", "ẞ lower cased but not replaced" ]
          |> List.map testSimpleNormalization
          |> testList "Code Normalization"

          let testNormalization (termName: string, expectedVariants: string list, name: string) =
              testCase $"{name} - Normalizing '{termName}' produces {expectedVariants}"
              <| fun () ->
                  let variants = normalize termName |> List.ofSeq

                  let normalizedExpected =
                      expectedVariants |> List.map _.Normalize(System.Text.NormalizationForm.FormKD)

                  test <@ variants = normalizedExpected @>

          [ "Term", [ "term" ], "Simple"
            "Terms", [ "term" ], "Plural Simple"
            "Multi word", [ "multiword" ], "Two words"
            "Many words here", [ "manywordshere" ], "Two words"
            "snake_case", [ "snakecase" ], "Snake Case"
            "kebab-case", [ "kebabcase" ], "Kebab Case"
            "Père", [ "pere"; "père" ], "Diacritic removed"
            "Auslöser", [ "ausloeser"; "ausloser"; "auslöser" ], "Umlaut removed and replaced with e (German)"
            "Straße", [ "strasse"; "straße" ], "ß replaced with ss and kept"
            "STRAẞE", [ "strasse"; "straße" ], "ẞ replaced with ss and kept" ]
          |> List.map testNormalization
          |> testList "Term Normalization"

          ]
