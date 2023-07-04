module Contextive.LanguageServer.Rendering

open Contextive.Core

let private emojifyTerm t = "📗 " + t
let private emphasise t = $"`{t}`"

let private doubleBlankLine = $"\n\n"

let private renderDefinition d =
    match d with
    | None -> "_undefined_"
    | Some(def) -> def

let private renderName name = name |> emphasise |> emojifyTerm

let private renderAliases (t: Definitions.Term) =
    match t.Aliases with
    | null -> None
    | aliases -> aliases |> Seq.map (fun alias -> $"_{alias}_") |> String.concat ", " |> Some

let private renderAliasLine (t: Definitions.Term) =
    match renderAliases t with
    | None -> None
    | Some aliases -> Some $"_Aliases_: {aliases}"

let private concatIfExists (separator: string) (lines: string option seq) =
    match lines |> Seq.choose id with
    | Seq.Empty -> None
    | s -> s |> String.concat separator |> Some

let private concatWithNewLinesIfExists = concatIfExists doubleBlankLine

let private renderTermDefinition (term: Definitions.Term) =
    seq {
        Some $"{renderName term.Name}: {renderDefinition term.Definition}"
        renderAliasLine term
    }
    |> concatIfExists "  \n"
    |> Seq.singleton

let private speechify usageExample = $"💬 \"{usageExample}\""

let private renderTermUsageExamples (t: Definitions.Term) =
    t.Examples
    |> Seq.map speechify
    |> Seq.append (Seq.singleton $"#### {emphasise t.Name} Usage Examples:")
    |> Seq.map Some

let private renderUsageExamples =
    function
    | { Definitions.Term.Examples = null } -> Seq.empty
    | t -> renderTermUsageExamples t

let renderTerm (terms: Definitions.Term seq) =
    [ renderTermDefinition; renderUsageExamples ]
    |> Seq.collect (fun p -> terms |> Seq.collect p)
    |> concatWithNewLinesIfExists

let private renderContextHeading (context: Definitions.Context) =
    match context.Name with
    | null
    | "" -> None
    | _ -> Some $"### 💠 {context.Name} Context"

let private renderContextDomainVisionStatement (context: Definitions.Context) =
    match context.DomainVisionStatement with
    | null
    | "" -> None
    | _ -> Some $"_Vision: {context.DomainVisionStatement}_"

let private renderContext (context: Definitions.Context) =
    let terms = context.Terms

    if Seq.length terms = 0 then
        None
    else
        [ renderTerm terms ]
        |> Seq.append (
            [ renderContextHeading; renderContextDomainVisionStatement ]
            |> Seq.map (fun f -> f context)
        )
        |> concatWithNewLinesIfExists

let private ContextSeparator = $"{doubleBlankLine}***{doubleBlankLine}"

let renderContexts (contexts: Definitions.FindResult) =
    contexts |> Seq.map renderContext |> concatIfExists ContextSeparator
