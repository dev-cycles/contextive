module Contextive.LanguageServer.Completion

open System.Linq
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private completionList labels =
    CompletionList(labels |> Seq.map (fun l -> CompletionItem(Label=l)), isIncomplete=true)

let private (|CamelCase|_|) (ct:string) = if System.Char.IsLower(ct.FirstOrDefault()) then Some () else None
let private (|PascalCase|_|) (ct:string) = if System.Char.IsUpper(ct.FirstOrDefault()) then Some () else None
let private (|UpperCase|_|) (ct:string) = if ct.Length > 1 && ct |> Seq.forall (System.Char.IsUpper) then Some () else None

let private termFilter _ = true

let upper (s:string) = s.ToUpper()
let lower (s:string) = s.ToLower()
let title (s:string) = s.Substring(0,1).ToUpper() + s.Substring(1)

let private wordToString (caseTemplate:string option) (word:string) = 
    match caseTemplate with
    | None -> word
    | Some(UpperCase) -> word |> upper
    | Some(PascalCase) -> word |> title
    | Some(CamelCase) -> word |> lower
    | _ -> word


let transform ((separator, headTransformer, tailTransformer):(string * (string -> string) * (string -> string))) (words: string seq) =
    Seq.append
        [words |> Seq.head |> headTransformer]
        (words |> Seq.tail |> Seq.map tailTransformer)
    |> String.concat separator

let wordCombinations (transformers:(string * (string -> string) * (string -> string)) seq) (words:string seq) =
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

let termToListOptions (caseTemplate: string option) (t:Definitions.Term) =
    let word = t.Name
    let words = Words.splitIntoParts <| Some word
    if (Seq.length words > 1) then
        multiWordToString caseTemplate words
    else
        seq { wordToString caseTemplate word }

let handler (termFinder: Definitions.Finder) (wordsGetter: TextDocument.WordGetter) (p:CompletionParams) (hc:CompletionCapability) _ =
    async {
        let caseTemplate = 
            match p.TextDocument with
            | null -> None
            | _ -> wordsGetter (p.TextDocument.Uri.ToUri()) p.Position
        let termToListOptionsWithCase = termToListOptions caseTemplate
        let uri = p.TextDocument.Uri.ToString()
        let! findResult = termFinder uri termFilter
        let labels = findResult
                     |> Definitions.FindResult.allTerms
                     |> Seq.collect termToListOptionsWithCase
        return completionList labels
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:CompletionCapability) (cc:ClientCapabilities)  =
    CompletionRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<CompletionRegistrationOptions, CompletionCapability>(registrationOptionsProvider)