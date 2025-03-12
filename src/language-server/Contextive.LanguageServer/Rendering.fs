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

let private renderAliases (t: GlossaryFile.Term) =
    match t.Aliases with
    | null -> None
    | aliases -> aliases |> Seq.map (fun alias -> $"_{alias}_") |> String.concat ", " |> Some

let private renderAliasLine (t: GlossaryFile.Term) =
    match renderAliases t with
    | None -> None
    | Some aliases -> Some $"_Aliases_: {aliases}"

let private concatIfExists (separator: string) (lines: string option seq) =
    match lines |> Seq.choose id with
    | Seq.Empty -> None
    | s -> s |> String.concat separator |> Some

let private concatWithNewLinesIfExists = concatIfExists doubleBlankLine

let private renderTermDefinition (term: GlossaryFile.Term) =
    seq {
        Some $"{renderName term.Name}: {renderDefinition term.Definition}"
        renderAliasLine term
    }
    |> concatIfExists "  \n"
    |> Seq.singleton

let private speechify (usageExample: string) = $"💬 \"{usageExample.Trim()}\""

let private renderTermUsageExamples (t: GlossaryFile.Term) =
    t.Examples
    |> Seq.map speechify
    |> Seq.append (Seq.singleton $"#### {emphasise t.Name} Usage Examples:")
    |> Seq.map Some

let private renderUsageExamples =
    function
    | { GlossaryFile.Term.Examples = null } -> Seq.empty
    | t -> renderTermUsageExamples t

let renderTerm (terms: GlossaryFile.Term seq) =
    [ renderTermDefinition; renderUsageExamples ]
    |> Seq.collect (fun p -> terms |> Seq.collect p)
    |> concatWithNewLinesIfExists

let private renderContextHeading (context: GlossaryFile.Context) =
    match context.Name with
    | null
    | "" -> None
    | _ -> Some $"### 💠 {context.Name} Context"

let private renderContextDomainVisionStatement (context: GlossaryFile.Context) =
    match context.DomainVisionStatement with
    | null
    | "" -> None
    | _ -> Some $"_Vision: {context.DomainVisionStatement.TrimEnd()}_"

let private renderContext (context: GlossaryFile.Context) =
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

let renderContexts (contexts: GlossaryFile.FindResult) =
    contexts |> Seq.map renderContext |> concatIfExists ContextSeparator
