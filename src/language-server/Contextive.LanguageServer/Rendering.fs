module Contextive.LanguageServer.Rendering

open Contextive.Core

let private emojify t = "📗 " + t
let private emphasise t = $"`{t}`"

let private doubleBlankLine = $"\n\n"

let private define d =
    match d with
    | None -> "_undefined_"
    | Some(def) -> def

let private getHoverDefinition (term: Definitions.Term) =
    [ term.Name |> emphasise |> emojify; term.Definition |> define ]
    |> String.concat ": "
    |> Some
    |> Seq.singleton

let private speechify usageExample = $"💬 \"{usageExample}\""

let private hoverUsageExamplesToMarkdown (t: Definitions.Term) =
    t.Examples
    |> Seq.map speechify
    |> Seq.append (Seq.singleton $"#### {emphasise t.Name} Usage Examples:")
    |> Seq.map Some

let private getHoverUsageExamples =
    function
    | { Definitions.Term.Examples = null } -> Seq.empty
    | t -> hoverUsageExamplesToMarkdown t

let private concatIfExists (separator: string) (lines: string option seq) =
    match lines |> Seq.choose id with
    | Seq.Empty -> None
    | s -> s |> String.concat separator |> Some

let private concatWithNewLinesIfExists = concatIfExists doubleBlankLine

let getTermHoverContent (terms: Definitions.Term seq) =
    [ getHoverDefinition; getHoverUsageExamples ]
    |> Seq.collect (fun p -> terms |> Seq.collect p)
    |> concatWithNewLinesIfExists

let private getContextHeading (context: Definitions.Context) =
    match context.Name with
    | null
    | "" -> None
    | _ -> Some $"### 💠 {context.Name} Context"

let private getContextDomainVisionStatement (context: Definitions.Context) =
    match context.DomainVisionStatement with
    | null
    | "" -> None
    | _ -> Some $"_Vision: {context.DomainVisionStatement}_"

let private getContextHover (context: Definitions.Context) =
    let terms = context.Terms

    if Seq.length terms = 0 then
        None
    else
        [ getTermHoverContent terms ]
        |> Seq.append (
            [ getContextHeading; getContextDomainVisionStatement ]
            |> Seq.map (fun f -> f context)
        )
        |> concatWithNewLinesIfExists

let private ContextSeparator = $"{doubleBlankLine}***{doubleBlankLine}"

let getContextsHoverContent (contexts: Definitions.FindResult) =
    contexts |> Seq.map getContextHover |> concatIfExists ContextSeparator
