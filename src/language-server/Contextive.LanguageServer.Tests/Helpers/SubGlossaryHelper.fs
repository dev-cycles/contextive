module Contextive.LanguageServer.Tests.Helpers.GlossaryHelper

open Contextive.Core.GlossaryFile

let termNamesToTerms (terms: string list) =
    terms |> Seq.map (fun t -> { Term.Default with Name = t })

let allContextsWithTerms (terms: Term seq) (contexts: Context seq) =
    Seq.map (Context.withTerms terms) contexts

let allContextsWithTermNames (termNames: string list) (contexts: Context seq) =
    allContextsWithTerms (termNamesToTerms termNames) contexts

let mockMultiContextDefinitionsFinder (contexts: Context seq) (terms: Term seq) =
    (fun _ f -> async { return allContextsWithTerms terms contexts |> f })

let mockDefinitionsFinder (contexts: Context) (terms: Term seq) =
    mockMultiContextDefinitionsFinder (seq { contexts }) terms

let mockMultiContextTermNamesFinder (contexts: Context seq) (termNames: string list) =
    termNames |> termNamesToTerms |> mockMultiContextDefinitionsFinder contexts

let mockTermNamesFinder (defaultContext: Context) (termNames: string list) =
    mockMultiContextTermNamesFinder (seq { defaultContext }) termNames

module FindResult =
    let allTerms (contexts: FindResult) : Term seq = contexts |> Seq.collect (_.Terms)
