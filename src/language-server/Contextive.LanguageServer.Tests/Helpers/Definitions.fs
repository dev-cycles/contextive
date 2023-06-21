module Contextive.LanguageServer.Tests.Helpers.Definitions

open Contextive.Core.Definitions

let mockDefinitionsFinder (defaultContext: Context) (definitions: Term seq) =
    (fun _ f -> async { return seq { defaultContext.WithTerms(Seq.filter f definitions) } })

let mockTermsFinder (defaultContext: Context) (terms: string list) =
    let termDefinitions = terms |> Seq.map (fun t -> { Term.Default with Name = t })
    mockDefinitionsFinder defaultContext termDefinitions

let mockMultiContextDefinitionsFinder (defaultContexts: Context seq) (definitions: Term seq) =
    (fun _ f -> async { return defaultContexts |> Seq.map (fun c -> c.WithTerms(Seq.filter f definitions)) })

let mockMultiContextTermsFinder (defaultContexts: Context seq) (terms: string list) =
    let termDefinitions = terms |> Seq.map (fun t -> { Term.Default with Name = t })
    mockMultiContextDefinitionsFinder defaultContexts termDefinitions
