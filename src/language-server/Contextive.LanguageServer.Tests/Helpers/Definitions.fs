module Contextive.LanguageServer.Tests.Helpers.Definitions

open Contextive.Core.Definitions

let mockDefinitionsFinder (defaultContext: Context) (definitions: Term seq) =
    (fun _ f -> async { return seq { Context.withTerms (Seq.filter f definitions) defaultContext } })

let mockTermsFinder (defaultContext: Context) (terms: string list) =
    let termDefinitions = terms |> Seq.map (fun t -> { Term.Default with Name = t })
    mockDefinitionsFinder defaultContext termDefinitions

let mockMultiContextDefinitionsFinder (defaultContexts: Context seq) (definitions: Term seq) =
    (fun _ f -> async { return defaultContexts |> Seq.map (Context.withTerms <| Seq.filter f definitions) })

let mockMultiContextTermsFinder (defaultContexts: Context seq) (terms: string list) =
    let termDefinitions = terms |> Seq.map (fun t -> { Term.Default with Name = t })
    mockMultiContextDefinitionsFinder defaultContexts termDefinitions
