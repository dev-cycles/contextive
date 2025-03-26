module Contextive.LanguageServer.Hover

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open Contextive.Core

module private Filtering =
    let private termEqualsToken
        (term: GlossaryFile.Term)
        (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms)
        =
        fst tokenAndCandidateTerms |> GlossaryFile.Term.equals term

    let private termInCandidates
        (term: GlossaryFile.Term)
        (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms)
        =
        (snd tokenAndCandidateTerms) |> Seq.exists (GlossaryFile.Term.equals term)

    let private removeLessRelevantTerms
        (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq)
        (terms: GlossaryFile.Term seq)
        =
        let exactTerms =
            tokenAndCandidateTerms
            |> Seq.allPairs terms
            |> Seq.filter (fun (t, tokenAndCandidates) -> termEqualsToken t tokenAndCandidates)

        let relevantTerms =
            exactTerms
            |> Seq.filter (fun (t, tokenAndCandidates) ->
                exactTerms
                |> Seq.except (seq { (t, tokenAndCandidates) })
                |> Seq.exists (fun (_, w) -> termInCandidates t w)
                |> not)
            |> Seq.map fst

        match relevantTerms with
        | Seq.Empty -> terms
        | _ -> relevantTerms

    let findMatchingTerms (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq) =
        Seq.filter (fun t ->
            let candidateMatchesTerm = termEqualsToken t
            tokenAndCandidateTerms |> Seq.exists candidateMatchesTerm)

    let termFilterForCandidateTerms tokenAndCandidateTerms =
        Seq.map (fun (c: GlossaryFile.Context) ->
            GlossaryFile.Context.withTerms
                (c.Terms
                 |> findMatchingTerms tokenAndCandidateTerms
                 |> removeLessRelevantTerms tokenAndCandidateTerms)
                c)

module private TextDocument =

    let getTokenAtPosition (p: HoverParams) (tokenFinder: TextDocument.TokenFinder) =
        match p.TextDocument with
        | null -> None
        | document -> tokenFinder document.Uri p.Position

module private Lsp =
    let markupContent content =
        MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

    let noHoverResult = null

let private hoverResult (contexts: GlossaryFile.FindResult) =
    match Rendering.renderContexts contexts with
    | None -> Lsp.noHoverResult
    | Some(c) -> Hover(Contents = (c |> Lsp.markupContent))

let private hoverContentForToken
    (uri: string)
    (termFinder: GlossaryFile.Finder)
    (tokensAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq)
    =
    async {
        let! findResult = termFinder uri (Filtering.termFilterForCandidateTerms tokensAndCandidateTerms)

        return
            if Seq.isEmpty findResult then
                Lsp.noHoverResult
            else
                hoverResult findResult
    }

let handler
    (termFinder: GlossaryFile.Finder)
    (tokenFinder: TextDocument.TokenFinder)
    (p: HoverParams)
    (_: HoverCapability)
    _
    =
    async {
        return!
            match TextDocument.getTokenAtPosition p tokenFinder with
            | None -> async { return Lsp.noHoverResult }
            | tokenAtPosition ->
                let uriPath =
                    try
                        p.TextDocument.Uri.ToUri().LocalPath
                    with _ ->
                        let dp = p.TextDocument.Uri.ToString()

                        Serilog.Log.Logger.Error
                            $"Unable to identify local path of '{dp}', using as is, which may not match glossary locations."

                        dp

                tokenAtPosition
                |> CandidateTerms.tokenToTokenAndCandidateTerms
                |> hoverContentForToken uriPath termFinder
    }
    |> Async.StartAsTask

let private registrationOptionsProvider (_: HoverCapability) (_: ClientCapabilities) = HoverRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability> registrationOptionsProvider
