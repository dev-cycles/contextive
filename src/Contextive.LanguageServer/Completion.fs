module Contextive.LanguageServer.Completion

open System.Linq
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

type CompletionItemData = {Label: string; Documentation: string option}

type CompletionData = {ContextName: string; CompletionItems: CompletionItemData seq}

let private getDetailFromContextName contextName = 
    match contextName with
    | null | "" -> null
    | _ -> $"{contextName} Context"

let private toMarkup str = 
    match str with
    | None -> null
    | Some(s) -> StringOrMarkupContent(markupContent=MarkupContent(Kind=MarkupKind.Markdown,Value=s))

let private toCompletionItem (detail: string) ({Label=label;Documentation=documentation}:CompletionItemData) =
    CompletionItem(
        Label=label,
        Detail=detail,
        Documentation=toMarkup documentation,
        Kind=CompletionItemKind.Reference
    )

let private toCompletionItemList {ContextName=contextName; CompletionItems=completionItems} =
    let toCompletionItemWithDetail =
        getDetailFromContextName contextName
        |> toCompletionItem
    completionItems
    |> Seq.map toCompletionItemWithDetail

let private toCompletionList (insertTexts:CompletionData seq) =
    CompletionList(
        insertTexts |> Seq.collect toCompletionItemList,
        isIncomplete=true)

let private (|CamelCase|_|) (ct:string) = if System.Char.IsLower(ct.FirstOrDefault()) then Some () else None
let private (|PascalCase|_|) (ct:string) = if System.Char.IsUpper(ct.FirstOrDefault()) then Some () else None
let private (|UpperCase|_|) (ct:string) = if ct.Length > 1 && ct |> Seq.forall (System.Char.IsUpper) then Some () else None

let private termFilter _ = true

let private upper (s:string) = s.ToUpper()
let private lower (s:string) = s.ToLower()
let private title (s:string) = s.Substring(0,1).ToUpper() + s.Substring(1)

let createCompletionItemData (term:Definitions.Term) label =
    {
        Label = label
        Documentation = Hover.getTermHoverContent [term]
    }

let private wordToString (caseTemplate:string option) (word:string) (term:Definitions.Term)= 
    match caseTemplate with
    | None -> word
    | Some(UpperCase) -> word |> upper
    | Some(PascalCase) -> word |> title
    | Some(CamelCase) -> word |> lower
    | _ -> word
    |> createCompletionItemData term

let private transform ((separator, headTransformer, tailTransformer):(string * (string -> string) * (string -> string))) (words: string seq) =
    Seq.append
        [words |> Seq.head |> headTransformer]
        (words |> Seq.tail |> Seq.map tailTransformer)
    |> String.concat separator

let private wordCombinations (transformers:(string * (string -> string) * (string -> string)) seq) (words:string seq) (term:Definitions.Term)=
    transformers
    |> Seq.map (fun transformer -> 
                    transform transformer words
                    |> createCompletionItemData term)

let private multiWordToString (caseTemplate: string option) (words:string seq) (term: Definitions.Term)=
    let snakeCase = ("_", lower, lower)
    let upperSnakeCase = ("_", upper, upper)
    let upperCase = ("", upper, upper)
    let camelCase = ("", lower, title)
    let pascalCase = ("", title, title)
    let wordCombinationGenerator =
        match caseTemplate with
        | Some(UpperCase)  -> words |> wordCombinations [upperCase  ; upperSnakeCase ]
        | Some(PascalCase) -> words |> wordCombinations [pascalCase ; upperSnakeCase ]
        | Some(CamelCase)  -> words |> wordCombinations [camelCase  ; snakeCase      ]
        | _                -> words |> wordCombinations [camelCase  ; pascalCase     ; snakeCase]
    wordCombinationGenerator term

let private termToListOptions (caseTemplate: string option) (term:Definitions.Term) : CompletionItemData seq =
    let word = term.Name
    let words = Words.splitIntoParts <| Some word
    if (Seq.length words > 1) then
        multiWordToString caseTemplate words term
    else
        seq { wordToString caseTemplate word term}

let private getContextCompletionLabelData (termToListOptionsWithCase) (context:Definitions.Context) =
    {
        ContextName=context.Name;
        CompletionItems=(context.Terms |> Seq.collect termToListOptionsWithCase)
    }

let private getCaseTemplate (wordsGetter: TextDocument.WordGetter) (textDocument:TextDocumentIdentifier) (position) =
    match textDocument with
    | null -> None
    | _ -> wordsGetter (textDocument.Uri.ToUri()) position

let handler (termFinder: Definitions.Finder) (wordsGetter: TextDocument.WordGetter) (p:CompletionParams) (hc:CompletionCapability) _ =
    async {
        let caseTemplate = getCaseTemplate wordsGetter (p.TextDocument) p.Position

        let getCompletionLabelDataWithCase = 
            termToListOptions caseTemplate
            |> getContextCompletionLabelData

        let uri = p.TextDocument.Uri.ToString()

        let! findResult = termFinder uri termFilter

        return
            findResult
            |> Seq.map getCompletionLabelDataWithCase
            |> toCompletionList
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:CompletionCapability) (cc:ClientCapabilities)  =
    CompletionRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<CompletionRegistrationOptions, CompletionCapability>(registrationOptionsProvider)