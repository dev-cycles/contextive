module Contextive.Core.GlossaryFile

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.Linq
open Contextive.Core.File
open System.ComponentModel.DataAnnotations
open System.Collections.Generic

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
        let normalizedCandidateTerm = Normalization.simpleNormalize candidateTerm

        Normalization.normalize termName
        |> Seq.exists (fun v -> v.Equals(normalizedCandidateTerm, System.StringComparison.InvariantCulture))


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
      Meta: IDictionary<string, string>
      Terms: ResizeArray<Term>
      [<YamlIgnore>]
      Index: IDictionary<string, Term list> }

module Context =

    let private indexTermByVariant (context: Context) (term: Term) (normalizedVariant: string) =
        if not <| context.Index.TryAdd(normalizedVariant, [ term ]) then
            let existingTermList = context.Index[normalizedVariant]
            let newTermList = term :: existingTermList
            context.Index[normalizedVariant] <- newTermList

    let private indexTermBy (context: Context) (term: Term) (key: string) =
        Normalization.normalize key |> Seq.iter (indexTermByVariant context term)

    let private indexTerm (context: Context) (term: Term) =
        indexTermBy context term term.Name

        if term.Aliases <> null then
            Seq.iter (indexTermBy context term) term.Aliases

    let index context =
        Seq.iter (indexTerm context) context.Terms

        context

    let withTerms (terms: Term seq) (context: Context) =
        { context with
            Terms = terms.ToList()
            Index = Dictionary<string, Term list>() }
        |> index

    let Default =
        { Name = ""
          DomainVisionStatement = ""
          Paths = ResizeArray()
          Meta = dict []
          Terms = ResizeArray<Term>()
          Index = Dictionary<string, Term list>() }

    let defaultWithTerms terms = Default |> withTerms terms

    let defaultIfNull context =
        if context |> box = null then Default else context

    let withDefaultTermsIfNull context =
        if context.Terms <> null then
            context
        else
            withTerms (ResizeArray<Term>()) context

    let withDefaultIndexIfNull context =
        if context.Index <> null then
            context
        else
            { context with
                Index = Dictionary<string, Term list>() }

    let fixNulls context =
        context |> defaultIfNull |> withDefaultTermsIfNull |> withDefaultIndexIfNull

[<CLIMutable>]
type GlossaryFile =
    { Imports: ResizeArray<string>
      Contexts: ResizeArray<Context> }

module GlossaryFile =
    let Default =
        { Imports = ResizeArray<string>()
          Contexts = ResizeArray<Context>() }

    let private replaceNullContextListWithDefault (glossaryFile: GlossaryFile) =
        match glossaryFile.Contexts with
        | null ->
            { glossaryFile with
                Contexts = Default.Contexts }
        | _ -> glossaryFile


    let private replaceNullImportsListWithDefault (glossaryFile: GlossaryFile) =
        match glossaryFile.Imports with
        | null ->
            { glossaryFile with
                Imports = Default.Imports }
        | _ -> glossaryFile

    let private fixContextNulls (glossaryFile: GlossaryFile) =
        { glossaryFile with
            Contexts = ResizeArray(glossaryFile.Contexts |> Seq.map Context.fixNulls) }

    let fixNulls (glossaryFile: GlossaryFile) =
        glossaryFile
        |> replaceNullContextListWithDefault
        |> replaceNullImportsListWithDefault
        |> fixContextNulls

    let index (glossaryFile: GlossaryFile) =
        Seq.iter (Context.index >> ignore) glossaryFile.Contexts
        glossaryFile

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

        let glossary = deserializer.Deserialize<GlossaryFile> yml

        match glossary |> box with
        | null -> Error(ParsingError "Glossary file is empty.")
        | _ -> glossary |> GlossaryFile.fixNulls |> GlossaryFile.index |> Ok
    with
    | :? YamlDotNet.Core.YamlException as e ->
        match e.InnerException with
        | :? ValidationException as ve ->
            Error(ValidationError $"{ve.Message} See line {e.Start.Line}, column {e.Start.Column}.")
        | _ ->
            let msg =
                if e.InnerException = null then
                    e.Message
                else
                    e.InnerException.Message

            Error(ParsingError $"Object starting line {e.Start.Line}, column {e.Start.Column} - {msg}")
    | e -> Error(ParsingError $"Unexpected parsing error: {e.ToString()}")

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
