module Contextive.Core.Definitions

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.Linq
open Contextive.Core.File
open System.ComponentModel.DataAnnotations

open Humanizer

[<CLIMutable>]
type Term =
    { [<Required>]
      Name: string
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
        let normalisedTerm =
            termName
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "")

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

    let withDefaultTermsIfNull context =
        if (context.Terms <> null) then
            context
        else
            withTerms (ResizeArray<Term>()) context

[<CLIMutable>]
type Definitions = { Contexts: ResizeArray<Context> }

module Definitions =
    let Default = { Contexts = new ResizeArray<Context>() }

type FindResult = Context seq
type Filter = FindResult -> FindResult
type Finder = string -> Filter -> Async<FindResult>

let private replaceNullsWithEmptyLists (definitions: Definitions) =
    { definitions with
        Contexts = ResizeArray(Seq.map Context.withDefaultTermsIfNull definitions.Contexts) }

let deserialize (yml: string) =
    try
        let deserializer =
            (new DeserializerBuilder())
                .WithNodeDeserializer(ValidatingDeserializer.factory, ValidatingDeserializer.where)
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()

        let definitions = deserializer.Deserialize<Definitions>(yml)

        match definitions |> box with
        | null -> Error(ParsingError("Definitions file is empty."))
        | _ -> Ok(definitions |> replaceNullsWithEmptyLists)
    with :? YamlDotNet.Core.YamlException as e ->
        match e.InnerException with
        | :? ValidationException as ve ->
            Error(ValidationError($"{ve.Message} See line {e.Start.Line}, column {e.Start.Column}."))
        | _ ->
            let msg =
                if e.InnerException = null then
                    e.Message
                else
                    e.InnerException.Message

            Error(ParsingError($"Object starting line {e.Start.Line}, column {e.Start.Column} - {msg}"))

open YamlDotNet.Core
open YamlDotNet.Core.Events

type OptionStringTypeConverter() =
    interface IYamlTypeConverter with
        member this.Accepts(``type``: System.Type) : bool =
            ``type``.FullName = (typeof<option<string>>).FullName

        member this.ReadYaml(parser: YamlDotNet.Core.IParser, ``type``: System.Type, _: ObjectDeserializer) : obj =
            let value = parser.Consume<Scalar>().Value
            if value = null then None else Some value

        member this.WriteYaml
            (emitter: YamlDotNet.Core.IEmitter, value: obj, ``type``: System.Type, _: ObjectSerializer)
            : unit =
            match (value :?> option<string>) with
            | None -> emitter.Emit(new Scalar(""))
            | Some v -> emitter.Emit(new Scalar(v))


let serialize (definitions: Definitions) =
    let serializer =
        (new SerializerBuilder())
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitEmptyCollections)
            .WithTypeConverter(OptionStringTypeConverter())
            .WithIndentedSequences()
            .Build()

    serializer.Serialize definitions
