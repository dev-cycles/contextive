module Contextive.LanguageServer.Tests.Helpers.Definitions

open Contextive.Core.Definitions

let mockMultiContextDefinitionsFinder (defaultContexts: Context seq) (definitions: Term seq) =
    (fun _ f -> async { return Seq.map (Context.withTerms definitions) defaultContexts |> f })

let mockDefinitionsFinder (defaultContext: Context) (definitions: Term seq) =
    mockMultiContextDefinitionsFinder (seq { defaultContext }) definitions

let mockMultiContextTermsFinder (defaultContexts: Context seq) (terms: string list) =
    let termDefinitions = terms |> Seq.map (fun t -> { Term.Default with Name = t })
    mockMultiContextDefinitionsFinder defaultContexts termDefinitions

let mockTermsFinder (defaultContext: Context) (terms: string list) =
    mockMultiContextTermsFinder (seq { defaultContext }) terms

module FindResult =
    let allTerms (contexts: FindResult) : Term seq =
        contexts |> Seq.collect (fun c -> c.Terms)
