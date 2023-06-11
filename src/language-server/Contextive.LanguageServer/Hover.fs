module Contextive.LanguageServer.Hover

open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities
open Contextive.Core
open Humanizer

let private (|EmptySeq|_|) a = if Seq.isEmpty a then Some () else None

let private markupContent content = 
    MarkedStringsOrMarkupContent(markupContent = MarkupContent(Kind = MarkupKind.Markdown, Value = content))

let private noHoverResult = null

let private termEquals (term:Definitions.Term) (candidateTerm:string) =
    let normalisedTerm = term.Name.Replace(" ", "")
    let singularEquals = normalisedTerm.Equals(candidateTerm, System.StringComparison.InvariantCultureIgnoreCase)
    let singularCandidate = candidateTerm.Singularize(false, false)
    let pluralEquals = normalisedTerm.Equals(singularCandidate, System.StringComparison.InvariantCultureIgnoreCase)
    singularEquals || pluralEquals

let private termEqualsCandidate (term:Definitions.Term) (tokenAndCandidateTerms:CandidateTerms.TokenAndCandidateTerms) = fst tokenAndCandidateTerms |> termEquals term
let private termInCandidates (term:Definitions.Term) (tokenAndCandidateTerms:CandidateTerms.TokenAndCandidateTerms) = (snd tokenAndCandidateTerms) |> Seq.exists (termEquals term)

let private termFilterForCandidateTerms tokenAndCandidateTerms =
    fun (t:Definitions.Term) -> 
        let candidateMatchesTerm = termEqualsCandidate t
        tokenAndCandidateTerms |> Seq.exists candidateMatchesTerm

let private filterRelevantTerms (terms: Definitions.Term seq) (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq) =
    let exactTerms =
        tokenAndCandidateTerms
        |> Seq.allPairs terms
        |> Seq.filter (fun (t, w) -> termEqualsCandidate t w)
    let relevantTerms =
        exactTerms
        |> Seq.filter (fun (t, wAndP) -> 
            exactTerms
            |> Seq.except (seq {(t, wAndP)})
            |> Seq.exists (fun (_, w) -> termInCandidates t w)
            |> not)
        |> Seq.map fst
    match relevantTerms with
    | EmptySeq -> terms
    | _ -> relevantTerms

let private getTokenAtPosition (p:HoverParams) (tokenFinder: TextDocument.TokenFinder) =
    match p.TextDocument with
    | null -> None
    | document -> tokenFinder (document.Uri) p.Position

let private emojify t = "📗 " + t
let private emphasise t = $"`{t}`"
let private define d =
    match d with
    | None -> "_undefined_"
    | Some(def) -> def

let private getHoverDefinition (term: Definitions.Term) =
    [term.Name |> emphasise |> emojify; term.Definition |> define]
    |> String.concat ": "
    |> Some
    |> Seq.singleton

let speechify usageExample = $"💬 \"{usageExample}\""

let private hoverUsageExamplesToMarkdown (t:Definitions.Term) =
    t.Examples
    |> Seq.map speechify
    |> Seq.append (Seq.singleton $"#### {emphasise t.Name} Usage Examples:")
    |> Seq.map Some

let private getHoverUsageExamples = 
    function | {Definitions.Term.Examples = null} -> Seq.empty
             | t -> hoverUsageExamplesToMarkdown t

let concatIfExists (separator:string) (lines:string option seq) =
    match lines |> Seq.choose id with
    | EmptySeq -> None
    | s -> s |> String.concat separator |> Some

let concatWithNewLinesIfExists = concatIfExists "\n\n"

let getTermHoverContent (terms: Definitions.Term seq) =
    [getHoverDefinition; getHoverUsageExamples]
    |> Seq.collect (fun p -> terms |> Seq.collect p)
    |> concatWithNewLinesIfExists

let private getContextHeading (context: Definitions.Context) =
    match context.Name with
    | null | "" -> None
    | _ -> Some $"### 💠 {context.Name} Context"

let private getContextDomainVisionStatement (context: Definitions.Context) =
    match context.DomainVisionStatement with
    | null | "" -> None
    | _ -> Some $"_Vision: {context.DomainVisionStatement}_"

let private getContextHover (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq) (context: Definitions.Context) =
    let relevantTerms = filterRelevantTerms context.Terms tokenAndCandidateTerms
    if Seq.length relevantTerms = 0 then
        None
    else
        [getTermHoverContent relevantTerms]
        |> Seq.append
            ([getContextHeading; getContextDomainVisionStatement]
            |> Seq.map (fun f -> f context))
        |> concatWithNewLinesIfExists

let private ContextSeparator = "\n\n***\n\n"

let private getContextsHoverContent (tokenAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq) (contexts: Definitions.FindResult)  =
    contexts
    |> Seq.map (getContextHover tokenAndCandidateTerms)
    |> concatIfExists ContextSeparator

let private hoverResult (tokensAndCandidateTerms:  CandidateTerms.TokenAndCandidateTerms seq) (contexts: Definitions.FindResult) =
    let content = getContextsHoverContent tokensAndCandidateTerms contexts
    match content with
    | None -> noHoverResult
    | Some(c) -> Hover(Contents = (c |> markupContent))

let private hoverContentForToken (uri:string) (termFinder:Definitions.Finder) (tokensAndCandidateTerms: CandidateTerms.TokenAndCandidateTerms seq) = async {
        let! findResult = termFinder uri (termFilterForCandidateTerms tokensAndCandidateTerms)
        return
            if Seq.isEmpty findResult then
                noHoverResult
            else
                hoverResult tokensAndCandidateTerms findResult
    }

let handler (termFinder: Definitions.Finder) (tokenFinder: TextDocument.TokenFinder) (p:HoverParams) (hc:HoverCapability) _ =
    async {
        let tokenAtPosition = getTokenAtPosition p tokenFinder
        return!
            match tokenAtPosition with
            | None -> async { return noHoverResult }
            | _ -> 
                let tokensAndCandidateTerms = CandidateTerms.tokenToTokenAndCandidateTerms tokenAtPosition
                hoverContentForToken (p.TextDocument.Uri.ToString()) termFinder tokensAndCandidateTerms
                    
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:HoverCapability) (cc:ClientCapabilities) =
    HoverRegistrationOptions()

let registrationOptions = RegistrationOptionsDelegate<HoverRegistrationOptions, HoverCapability>(registrationOptionsProvider)