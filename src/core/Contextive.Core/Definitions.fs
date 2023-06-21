module Contextive.Core.Definitions

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.Linq

[<CLIMutable>]
type Term =
    { Name: string
      Definition: string option
      Examples: ResizeArray<string>
      Aliases: ResizeArray<string> }

    static member Default =
        { Name = ""
          Definition = None
          Examples = null
          Aliases = null }

[<CLIMutable>]
type Context =
    { Name: string
      DomainVisionStatement: string
      Paths: ResizeArray<string>
      Terms: ResizeArray<Term> }

    static member Default =
        { Name = ""
          DomainVisionStatement = ""
          Paths = new ResizeArray<string>()
          Terms = new ResizeArray<Term>() }

    member this.WithTerms(terms: Term seq) = { this with Terms = terms.ToList() }

[<CLIMutable>]
type Definitions =
    { Contexts: ResizeArray<Context> }

    static member Default = { Contexts = new ResizeArray<Context>() }

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
