module Contextive.LanguageServer.Tests.Helpers.Fixtures

module One =
    let expectedTerms = seq [ "firstTerm"; "secondTerm"; "thirdTerm" ]

    let expectedCompletionLabels =
        seq
            [ "firstTerm"
              "FirstTerm"
              "first_term"
              "first-term"
              "secondTerm"
              "SecondTerm"
              "second_term"
              "second-term"
              "thirdTerm"
              "ThirdTerm"
              "third_term"
              "third-term" ]

    let path = System.IO.Path.Combine("fixtures", "completion_tests", "one.yml")

module Two =
    let expectedCompletionLabels =
        seq
            [ "word1"
              "Word1"
              "word_1"
              "word-1"
              "word2"
              "Word2"
              "word_2"
              "word-2"
              "word3"
              "Word3"
              "word_3"
              "word-3" ]

    let expectedCompletionLabelsPascal =
        seq [ "Word1"; "WORD_1"; "Word2"; "WORD_2"; "Word3"; "WORD_3" ]

    let expectedCompletionLabelsUPPER =
        seq [ "WORD1"; "WORD_1"; "WORD2"; "WORD_2"; "WORD3"; "WORD_3" ]

module Imports =
    let rootPath = System.IO.Path.Combine("fixtures", "import_tests")

    module Main =
        let path = System.IO.Path.Combine(rootPath, "main_folder", "main.glossary.yml")
