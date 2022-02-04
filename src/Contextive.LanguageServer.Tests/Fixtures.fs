module Contextive.LanguageServer.Tests.Fixtures

module One =
    let expectedTerms = seq ["firstTerm";"secondTerm";"thirdTerm"]
    let expectedCompletionLabels = seq ["firstTerm";"FirstTerm";"first_term";"secondTerm";"SecondTerm";"second_term";"thirdTerm";"ThirdTerm";"third_term"]

module Two =
    let expectedCompletionLabels = seq ["word1";"Word1";"word_1";"word2";"Word2";"word_2";"word3";"Word3";"word_3"]
    let expectedCompletionLabelsPascal = seq ["Word1";"WORD_1";"Word2";"WORD_2";"Word3";"WORD_3"]
    let expectedCompletionLabelsUPPER = seq ["WORD1";"WORD_1";"WORD2";"WORD_2";"WORD3";"WORD_3"]
