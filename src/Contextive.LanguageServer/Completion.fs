module Contextive.LanguageServer.Completion

open System.Linq
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

type CompletionData = {ContextName: string; Labels: seq<string>}

let private getDetailFromContextName contextName = 
    match contextName with
    | null | "" -> null
    | _ -> $"{contextName} Context"

let private toCompletionItem (detail: string) (label:string) =
    CompletionItem(
        Label=label,
        Detail=detail
    )

let private toCompletionItemList {ContextName=contextName; Labels=labels} =
    let toCompletionItemWithDetail =
        getDetailFromContextName contextName
        |> toCompletionItem
    labels
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

let private wordToString (caseTemplate:string option) (word:string) = 
    match caseTemplate with
    | None -> word
    | Some(UpperCase) -> word |> upper
    | Some(PascalCase) -> word |> title
    | Some(CamelCase) -> word |> lower
    | _ -> word


let private transform ((separator, headTransformer, tailTransformer):(string * (string -> string) * (string -> string))) (words: string seq) =
    Seq.append
        [words |> Seq.head |> headTransformer]
        (words |> Seq.tail |> Seq.map tailTransformer)
    |> String.concat separator

let private wordCombinations (transformers:(string * (string -> string) * (string -> string)) seq) (words:string seq) =
    transformers
    |> Seq.map (fun transformer -> transform transformer words)

let private multiWordToString (caseTemplate: string option) (words:string seq) =
    let snakeCase = ("_", lower, lower)
    let upperSnakeCase = ("_", upper, upper)
    let upperCase = ("", upper, upper)
    let camelCase = ("", lower, title)
    let pascalCase = ("", title, title)
    match caseTemplate with
    | Some(UpperCase)  -> words |> wordCombinations [upperCase  ; upperSnakeCase ]
    | Some(PascalCase) -> words |> wordCombinations [pascalCase ; upperSnakeCase ]
    | Some(CamelCase)  -> words |> wordCombinations [camelCase  ; snakeCase      ]
    | _                -> words |> wordCombinations [camelCase  ; pascalCase     ; snakeCase]

let private termToListOptions (caseTemplate: string option) (t:Definitions.Term) =
    let word = t.Name
    let words = Words.splitIntoParts <| Some word
    if (Seq.length words > 1) then
        multiWordToString caseTemplate words
    else
        seq { wordToString caseTemplate word }

let private getContextCompletionLabelData (termToListOptionsWithCase) (context:Definitions.Context) =
    {
        ContextName=context.Name;
        Labels=(context.Terms |> Seq.collect termToListOptionsWithCase)
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