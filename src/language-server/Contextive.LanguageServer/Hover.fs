module Contextive.LanguageServer.Hover

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open Contextive.Core

let private (|EmptySeq|_|) a = if Seq.isEmpty a then Some() else None

module Formatting =
    let private emojify t = "📗 " + t
    let private emphasise t = $"`{t}`"

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
        | EmptySeq -> None
        | s -> s |> String.concat separator |> Some

    let private concatWithNewLinesIfExists = concatIfExists "\n\n"

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

    let private getContextHover
        (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq)
        (context: Definitions.Context)
        =
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

    let private ContextSeparator = "\n\n***\n\n"

    let getContextsHoverContent
        (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq)
        (contexts: Definitions.FindResult)
        =
        contexts
        |> Seq.map (getContextHover tokenAndCandidateTerms)
        |> concatIfExists ContextSeparator

module private Filtering =
    let private termEqualsCandidate
        (term: Definitions.Term)
        (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms)
        =
        fst tokenAndCandidateTerms |> Definitions.Term.equals term

    let private termInCandidates
        (term: Definitions.Term)
        (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms)
        =
        (snd tokenAndCandidateTerms) |> Seq.exists (Definitions.Term.equals term)

    let private filterRelevantTerms
        (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq)
        (terms: Definitions.Term seq)
        =
        let exactTerms =
            tokenAndCandidateTerms
            |> Seq.allPairs terms
            |> Seq.filter (fun (t, w) -> termEqualsCandidate t w)

        let relevantTerms =
            exactTerms
            |> Seq.filter (fun (t, wAndP) ->
                exactTerms
                |> Seq.except (seq { (t, wAndP) })
                |> Seq.exists (fun (_, w) -> termInCandidates t w)
                |> not)
            |> Seq.map fst

        match relevantTerms with
        | EmptySeq -> terms
        | _ -> relevantTerms

    let termFilterForCandidateTerms tokenAndCandidateTerms =
        fun (contexts: Definitions.FindResult) ->
            contexts
            |> Seq.map (fun c ->
                Definitions.Context.withTerms
                    (c.Terms
                     |> Seq.filter (fun t ->
                         let candidateMatchesTerm = termEqualsCandidate t
                         tokenAndCandidateTerms |> Seq.exists candidateMatchesTerm)
                     |> filterRelevantTerms tokenAndCandidateTerms)
                    c)

module private TextDocument =

    let getTokenAtPosition (p: HoverParams) (tokenFinder: TextDocument.TokenFinder) =
        match p.TextDocument with
        | null -> None
        | document -> tokenFinder (document.Uri) p.Position

module private Lsp =
    let markupContent content =
        MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

    let noHoverResult = null

let private hoverResult
    (tokensAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq)
    (contexts: Definitions.FindResult)
    =
    let content = Formatting.getContextsHoverContent tokensAndCandidateTerms contexts

    match content with
    | None -> Lsp.noHoverResult
    | Some(c) -> Hover(Contents = (c |> Lsp.markupContent))

let private hoverContentForToken
    (uri: string)
    (termFinder: Definitions.Finder)
    (tokensAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq)
    =
    async {
        let! findResult = termFinder uri (Filtering.termFilterForCandidateTerms tokensAndCandidateTerms)

        return
            if Seq.isEmpty findResult then
                Lsp.noHoverResult
            else
                hoverResult tokensAndCandidateTerms findResult
    }

let handler
    (termFinder: Definitions.Finder)
    (tokenFinder: TextDocument.TokenFinder)
    (p: HoverParams)
    (hc: HoverCapability)
    _
    =
    async {
        let tokenAtPosition = TextDocument.getTokenAtPosition p tokenFinder

        return!
            match tokenAtPosition with
            | None -> async { return Lsp.noHoverResult }
            | _ ->
                let tokensAndCandidateTerms =
                    CandidateTerms.tokenToTokenAndCandidateTerms tokenAtPosition

                hoverContentForToken (p.TextDocument.Uri.ToString()) termFinder tokensAndCandidateTerms

    }
    |> Async.StartAsTask

let private registrationOptionsProvider (hc: HoverCapability) (cc: ClientCapabilities) = HoverRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)
