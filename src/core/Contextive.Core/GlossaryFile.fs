module Contextive.Core.GlossaryFile

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.Linq
open Contextive.Core.File
open System.ComponentModel.DataAnnotations
open System.Collections.Generic

open Humanizer

[<CLIMutable>]
type Term =
    { [<Required>]
      Name: string
      Definition: string option
      Examples: ResizeArray<string>
      Aliases: ResizeArray<string>
      Meta: IDictionary<string, string> }

module Term =
    let Default =
        { Name = ""
          Definition = None
          Examples = null
          Aliases = null
          Meta = null }

    let private nameEquals (candidateTerm: string) (termName: string) =
        let normalisedTerm = termName.Replace(" ", "").Replace("_", "").Replace("-", "")

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
          Paths = ResizeArray<string>()
          Terms = ResizeArray<Term>() }

    let defaultWithTerms terms = Default |> withTerms terms

    let defaultIfNull context =
        if context |> box = null then Default else context

    let withDefaultTermsIfNull context =
        if context.Terms <> null then
            context
        else
            withTerms (ResizeArray<Term>()) context

[<CLIMutable>]
type GlossaryFile = { Contexts: ResizeArray<Context> }

module GlossaryFile =
    let Default = { Contexts = ResizeArray<Context>() }

    let private replaceNullContextsWithDefaults (glossaryFile: GlossaryFile) =
        { glossaryFile with
            Contexts = ResizeArray(glossaryFile.Contexts |> Seq.map Context.defaultIfNull) }

    let private replaceNullContextListWithDefault (glossaryFile: GlossaryFile) =
        match glossaryFile.Contexts with
        | null -> Default
        | _ -> glossaryFile

    let private replaceNullTermsWithDefault (glossaryFile: GlossaryFile) =
        { glossaryFile with
            Contexts = ResizeArray(Seq.map Context.withDefaultTermsIfNull glossaryFile.Contexts) }

    let fixNulls (glossaryFile: GlossaryFile) =
        glossaryFile
        |> replaceNullContextListWithDefault
        |> replaceNullContextsWithDefaults
        |> replaceNullTermsWithDefault

type FindResult = Context seq
type Filter = FindResult -> FindResult
type Finder = string -> Filter -> Async<FindResult>


let deserialize (yml: string) =
    try
        let deserializer =
            DeserializerBuilder()
                .WithNodeDeserializer(ValidatingDeserializer.factory, ValidatingDeserializer.where)
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()

        let glossary = deserializer.Deserialize<GlossaryFile>(yml)

        match glossary |> box with
        | null -> Error(ParsingError("Glossary file is empty."))
        | _ -> glossary |> GlossaryFile.fixNulls |> Ok
    with
    | :? YamlDotNet.Core.YamlException as e ->
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
    | e -> Error(ParsingError($"Unexpected parsing error: {e.ToString()}"))

open YamlDotNet.Core
open YamlDotNet.Core.Events

type OptionStringTypeConverter() =
    interface IYamlTypeConverter with
        member this.Accepts(``type``: System.Type) : bool =
            ``type``.FullName = typeof<option<string>>.FullName

        member this.ReadYaml(parser: IParser, _: System.Type, _: ObjectDeserializer) : obj =
            let value = parser.Consume<Scalar>().Value
            if value = null then None else Some value

        member this.WriteYaml(emitter: IEmitter, value: obj, _: System.Type, _: ObjectSerializer) : unit =
            match (value :?> option<string>) with
            | None -> emitter.Emit(Scalar(""))
            | Some v -> emitter.Emit(Scalar(v))


let serialize (glossaryFile: GlossaryFile) =
    let serializer =
        SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(
                DefaultValuesHandling.OmitEmptyCollections ||| DefaultValuesHandling.OmitNull
            )
            .WithTypeConverter(OptionStringTypeConverter())
            .WithIndentedSequences()
            .Build()

    serializer.Serialize glossaryFile
