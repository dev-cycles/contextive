module Contextive.LanguageServer.Rendering

open Contextive.Core

let private emojifyTerm t = $"### 📗 {t}"
let private emphasise t = $"`{t}`"

let private doubleBlankLine = "\n\n"

let private ContextSeparator = $"{doubleBlankLine}***{doubleBlankLine}"

let private renderDefinition d =
    match d with
    | None -> "> 📝 _undefined_"
    | Some(def) -> $"> 📝 {def}"

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

let private renderDefinitionWithAlias (t: GlossaryFile.Term) =
    seq {
        t.Definition |> renderDefinition |> Some
        renderAliasLine t
    }
    |> concatIfExists "  \n"

let private concatWithNewLinesIfExists = concatIfExists doubleBlankLine

let private speechify (usageExample: string) = $"💬 \"{usageExample.Trim()}\""

let private renderTermUsageExamples (t: GlossaryFile.Term) =
    t.Examples |> Seq.map speechify |> Seq.map Some

let private renderUsageExamples =
    function
    | { GlossaryFile.Term.Examples = null } -> Seq.empty
    | t -> renderTermUsageExamples t

let private renderTermMetadataKeyValues (t: GlossaryFile.Term) =
    t.Meta |> Seq.map (fun kvp -> $"{kvp.Key} {kvp.Value}" |> Some)

let private renderTermMetadata =
    function
    | { GlossaryFile.Term.Meta = null } -> Seq.empty
    | t -> renderTermMetadataKeyValues t

let private renderTermDefinition (term: GlossaryFile.Term) =
    seq {
        term.Name |> renderName |> Some |> Seq.singleton
        renderDefinitionWithAlias term |> Seq.singleton
        renderUsageExamples term
        renderTermMetadata term
    }
    |> Seq.collect id
    |> concatWithNewLinesIfExists

let renderTerm (terms: GlossaryFile.Term seq) =
    terms |> Seq.map renderTermDefinition |> concatIfExists ContextSeparator

let private renderContextHeading (context: GlossaryFile.Context) =
    match context.Name with
    | null
    | "" -> None
    | _ -> Some $"## 💠 {context.Name} Context"

let private renderContextDomainVisionStatement (context: GlossaryFile.Context) =
    match context.DomainVisionStatement with
    | null
    | "" -> None
    | _ -> Some $"_Vision: {context.DomainVisionStatement.TrimEnd()}_"

let private renderContextMetadataKeyValues (t: GlossaryFile.Context) =
    t.Meta |> Seq.map (fun kvp -> $"{kvp.Key} {kvp.Value}" |> Some)

let private renderContextMetadata =
    function
    | { GlossaryFile.Context.Meta = null } -> Seq.empty
    | t -> renderContextMetadataKeyValues t

let private renderContext (context: GlossaryFile.Context) =
    let terms = context.Terms

    if Seq.length terms = 0 then
        None
    else
        let allTerms = renderTerm terms |> Seq.singleton
        let metadata = renderContextMetadata context

        let header =
            seq {
                renderContextHeading
                renderContextDomainVisionStatement
            }
            |> Seq.map (fun f -> f context)

        let contextHeader =
            seq {
                header
                metadata
            }
            |> Seq.collect id
            |> concatWithNewLinesIfExists

        seq {
            contextHeader |> Seq.singleton
            allTerms
        }
        |> Seq.collect id
        |> concatIfExists ContextSeparator

let renderContexts (contexts: GlossaryFile.FindResult) =
    contexts |> Seq.map renderContext |> concatIfExists ContextSeparator
