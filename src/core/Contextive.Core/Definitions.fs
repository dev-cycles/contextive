module Contextive.Core.Definitions

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.Linq

open Humanizer

[<CLIMutable>]
type Term =
    { Name: string
      Definition: string option
      Examples: ResizeArray<string>
      Aliases: ResizeArray<string> }

module Term =
    let Default =
        { Name = ""
          Definition = None
          Examples = null
          Aliases = null }

    let private nameEquals (candidateTerm: string) (termName: string) =
        let normalisedTerm = termName.Replace(" ", "")

        let singularEquals =
            normalisedTerm.Equals(candidateTerm, System.StringComparison.InvariantCultureIgnoreCase)

        let singularCandidate = candidateTerm.Singularize(false, false)

        let pluralEquals =
            normalisedTerm.Equals(singularCandidate, System.StringComparison.InvariantCultureIgnoreCase)

        singularEquals || pluralEquals

    let private aliasesEqual (term: Term) (candidateTerm: string) =
        match term.Aliases with
        | null -> false
        | _ -> Seq.exists (nameEquals candidateTerm) term.Aliases

    let equals (term: Term) (candidateTerm: string) =
        nameEquals candidateTerm term.Name || aliasesEqual term candidateTerm

[<CLIMutable>]
type Context =
    { Name: string
      DomainVisionStatement: string
      Paths: ResizeArray<string>
      Terms: ResizeArray<Term> }

module Context =

    let withTerms (terms: Term seq) (context: Context) = { context with Terms = terms.ToList() }

    let Default =
        { Name = ""
          DomainVisionStatement = ""
          Paths = new ResizeArray<string>()
          Terms = new ResizeArray<Term>() }

    let defaultWithTerms terms = Default |> withTerms terms

[<CLIMutable>]
type Definitions = { Contexts: ResizeArray<Context> }

module Definitions =
    let Default = { Contexts = new ResizeArray<Context>() }

type Filter = Term -> bool
type FindResult = Context seq
type Finder = string -> Filter -> Async<FindResult>

module FindResult =
    let allTerms (contexts: FindResult) : Term seq =
        contexts |> Seq.collect (fun c -> c.Terms)

let deserialize (yml: string) =
    try
        let deserializer =
            (new DeserializerBuilder())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()

        let definitions = deserializer.Deserialize<Definitions>(yml)

        match definitions |> box with
        | null -> Error "Definitions file is empty."
        | _ -> Ok definitions
    with :? YamlDotNet.Core.YamlException as e ->
        let msg =
            if e.InnerException = null then
                e.Message
            else
                e.InnerException.Message

        Error $"Error parsing definitions file:  Object starting line {e.Start.Line}, column {e.Start.Column} - {msg}"
