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

    let findMatchingTermsInIndex (context: GlossaryFile.Context) =

        Seq.collect (fun (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms) ->
            let token = fst tokenAndCandidateTerms |> Normalization.simpleNormalize

            if context.Index.ContainsKey token then
                context.Index[token]
            else
                [])

    // For CJK tokens, find terms whose index key is a substring of the token.
    // Both token and keys are normalised with simpleNormalize (NFKD + lowercase +
    // Singularize). Singularize is a no-op for CJK text in Humanizer, so the
    // normalisation is effectively NFKD + lowercase on both sides.
    // cursorOffsetInToken restricts matches to keys that span the cursor position.
    let findMatchingTermsBySubstring (context: GlossaryFile.Context) (token: string) (cursorOffsetInToken: int) =
        let normalizedToken = Normalization.simpleNormalize token

        context.Index.Keys
        |> Seq.filter (fun key ->
            let idx = normalizedToken.IndexOf(key)
            idx >= 0 && cursorOffsetInToken >= idx && cursorOffsetInToken < idx + key.Length)
        |> Seq.collect (fun key -> context.Index[key])
        |> Seq.distinctBy (fun t -> t.Name)

    let termFilterForCandidateTermsWithIndex cursorOffsetInToken tokenAndCandidateTerms =
        Seq.map (fun (c: GlossaryFile.Context) ->

            let token = tokenAndCandidateTerms |> Seq.head |> fst

            let terms =
                if CandidateTerms.containsCJK token then
                    findMatchingTermsBySubstring c token cursorOffsetInToken
                else
                    findMatchingTermsInIndex c tokenAndCandidateTerms

            // For CJK substring matches, no term exactly equals the full token,
            // so removeLessRelevantTerms falls through to returning all matched terms.
            GlossaryFile.Context.withTerms (terms |> removeLessRelevantTerms tokenAndCandidateTerms) c)

module private TextDocument =

    let getTokenWithStartAtPosition (p: HoverParams) (tokenFinder: DocumentUri -> Position -> (string * int) option) =
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
    (cursorOffsetInToken: int)
    (tokensAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq)
    =
    async {
        let! findResult = termFinder uri (Filtering.termFilterForCandidateTermsWithIndex cursorOffsetInToken tokensAndCandidateTerms)

        return
            if Seq.isEmpty findResult then
                Lsp.noHoverResult
            else
                hoverResult findResult
    }

let handler
    (termFinder: GlossaryFile.Finder)
    (tokenFinder: DocumentUri -> Position -> (string * int) option)
    (p: HoverParams)
    (_: HoverCapability)
    _
    =
    async {
        return!
            match TextDocument.getTokenWithStartAtPosition p tokenFinder with
            | None -> async { return Lsp.noHoverResult }
            | Some(token, tokenStart) ->
                let cursorOffsetInToken = p.Position.Character - tokenStart

                let uriPath =
                    try
                        p.TextDocument.Uri.ToUri().LocalPath
                    with _ ->
                        let dp = p.TextDocument.Uri.ToString()

                        Serilog.Log.Logger.Error
                            $"Unable to identify local path of '{dp}', using as is, which may not match glossary locations."

                        dp

                Some token
                |> CandidateTerms.tokenToTokenAndCandidateTerms
                |> hoverContentForToken uriPath termFinder cursorOffsetInToken
    }
    |> Async.StartAsTask

let private registrationOptionsProvider (_: HoverCapability) (_: ClientCapabilities) = HoverRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability> registrationOptionsProvider
