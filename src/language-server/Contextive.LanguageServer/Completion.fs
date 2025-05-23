module Contextive.LanguageServer.Completion

open System.Linq
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open Contextive.Core

type CompletionItemData =
    { Label: string
      Documentation: string option }

type CompletionData =
    { ContextName: string
      CompletionItems: CompletionItemData seq }

let private getDetailFromContextName contextName =
    match contextName with
    | null
    | "" -> null
    | _ -> $"{contextName} Context"

let private toMarkup str =
    match str with
    | None -> null
    | Some(s) -> StringOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = s))

let private toCompletionItem
    (detail: string)
    ({ Label = label
       Documentation = documentation }: CompletionItemData)
    =
    CompletionItem(
        Label = label,
        Detail = detail,
        Documentation = toMarkup documentation,
        Kind = CompletionItemKind.Reference
    )

let private toCompletionItemList
    { ContextName = contextName
      CompletionItems = completionItems }
    =
    let toCompletionItemWithDetail =
        getDetailFromContextName contextName |> toCompletionItem

    completionItems |> Seq.map toCompletionItemWithDetail

let private toCompletionList (insertTexts: CompletionData seq) =
    CompletionList(insertTexts |> Seq.collect toCompletionItemList, isIncomplete = true)

let private (|CamelCase|_|) (ct: string) =
    if System.Char.IsLower(ct.FirstOrDefault()) then
        Some()
    else
        None

let private (|PascalCase|_|) (ct: string) =
    if System.Char.IsUpper(ct.FirstOrDefault()) then
        Some()
    else
        None

let private (|UpperCase|_|) (ct: string) =
    if ct.Length > 1 && ct |> Seq.forall System.Char.IsUpper then
        Some()
    else
        None

let private (|KebabCase|_|) (ct: string) =
    if ct.Contains("-") && ct.ToLower() = ct then
        Some()
    else
        None

[<Literal>]
let private MAX_TERMS_IN_COMPLETION_PER_CONTEXT = 60

let private termFilter
    (normalizedPrefix: string option)
    (contextSeq: GlossaryFile.FindResult)
    : GlossaryFile.FindResult =

    Seq.map
        (fun (context: GlossaryFile.Context) ->
            let terms =
                match normalizedPrefix with
                | Some np ->

                    let candidateKeys =
                        context.Index.Keys |> Seq.filter (fun k -> k.StartsWith np) |> Seq.toList

                    let candidateTerms =
                        candidateKeys |> Seq.collect (fun k -> context.Index[k]) |> Seq.toList

                    candidateTerms |> Seq.distinctBy (fun t -> t.Name)

                | None -> context.Terms

                |> Seq.truncate MAX_TERMS_IN_COMPLETION_PER_CONTEXT
                |> ResizeArray

            GlossaryFile.Context.withTerms terms context)
        contextSeq

let private upper (s: string) = s.ToUpper()
let private lower (s: string) = s.ToLower()

let private title (s: string) =
    s.Substring(0, 1).ToUpper() + s.Substring(1)

let createCompletionItemData (term: GlossaryFile.Term) label =
    { Label = label
      Documentation = Rendering.renderTerm [ term ] }

let private termToCaseMatchedCompletionData
    (caseTemplate: string option)
    (token: CandidateTerms.Token)
    (term: GlossaryFile.Term)
    =
    match caseTemplate with
    | None -> token
    | Some(UpperCase) -> token |> upper
    | Some(PascalCase) -> token |> title
    | Some(CamelCase) -> token |> lower
    | _ -> token
    |> createCompletionItemData term

let private transform
    ((separator, headTransformer, tailTransformer): string * (string -> string) * (string -> string))
    (candidateTerms: CandidateTerms.CandidateTerms)
    =
    Seq.append [ candidateTerms |> Seq.head |> headTransformer ] (candidateTerms |> Seq.tail |> Seq.map tailTransformer)
    |> String.concat separator

let private tokenCombinations
    (transformers: (string * (string -> string) * (string -> string)) seq)
    (candidateTerms: CandidateTerms.CandidateTerms)
    (term: GlossaryFile.Term)
    =
    transformers
    |> Seq.map (fun transformer -> transform transformer candidateTerms |> createCompletionItemData term)

let private candidateTermsToCaseMatchedCompletionData
    (caseTemplate: string option)
    (candidateTerms: CandidateTerms.CandidateTerms)
    (term: GlossaryFile.Term)
    =
    let snakeCase = ("_", lower, lower)
    let upperSnakeCase = ("_", upper, upper)
    let upperCase = ("", upper, upper)
    let camelCase = ("", lower, title)
    let pascalCase = ("", title, title)
    let kebabCase = ("-", lower, lower)

    let tokenCombinationGenerator =
        match caseTemplate with
        | Some(UpperCase) -> candidateTerms |> tokenCombinations [ upperCase; upperSnakeCase ]
        | Some(PascalCase) -> candidateTerms |> tokenCombinations [ pascalCase; upperSnakeCase ]
        | Some(CamelCase) -> candidateTerms |> tokenCombinations [ camelCase; snakeCase ]
        | Some(KebabCase) -> candidateTerms |> tokenCombinations [ kebabCase; snakeCase ]
        | _ ->
            candidateTerms
            |> tokenCombinations [ camelCase; pascalCase; snakeCase; kebabCase ]

    tokenCombinationGenerator term

let private termToListOptions (caseTemplate: string option) (term: GlossaryFile.Term) : CompletionItemData seq =
    let token = term.Name
    let candidateTerms = CandidateTerms.candidateTermsFromToken <| Some token

    if (Seq.length candidateTerms > 1) then
        candidateTermsToCaseMatchedCompletionData caseTemplate candidateTerms term
    else
        seq { termToCaseMatchedCompletionData caseTemplate token term }

let private getContextCompletionLabelData termToListOptionsWithCase (context: GlossaryFile.Context) =
    { ContextName = context.Name
      CompletionItems = (context.Terms |> Seq.collect termToListOptionsWithCase) }

let private getCaseTemplate (tokenFinder: TextDocument.TokenFinder) (textDocument: TextDocumentIdentifier) position =
    match textDocument with
    | null -> None
    | _ -> tokenFinder textDocument.Uri position

let handler
    (termFinder: GlossaryFile.Finder)
    (tokenFinder: TextDocument.TokenFinder)
    (p: CompletionParams)
    (_: CompletionCapability)
    _
    =
    async {
        let caseTemplate = getCaseTemplate tokenFinder p.TextDocument p.Position

        let getCompletionLabelDataWithCase =
            termToListOptions caseTemplate |> getContextCompletionLabelData

        let uri = p.TextDocument.Uri.ToUri().LocalPath

        let normalizedPrefix = Option.map Normalization.simpleNormalize caseTemplate

        let! findResult = termFinder uri (termFilter normalizedPrefix)

        return findResult |> Seq.map getCompletionLabelDataWithCase |> toCompletionList
    }
    |> Async.StartAsTask

let private registrationOptionsProvider (_: CompletionCapability) (_: ClientCapabilities) =
    CompletionRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<CompletionRegistrationOptions, CompletionCapability> registrationOptionsProvider
