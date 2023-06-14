module Contextive.Core.CandidateTerms

open System.Text.RegularExpressions
open System.Linq
open Humanizer

// Token is a punctuation delimited text from a source file
type Token = string
// Candidate term represents a subset or all of a token that could match a term
// from the dictionary
type CandidateTerm = string
type CandidateTerms = CandidateTerm seq
// A tuple of the original token and the contained candidate terms
type TokenAndCandidateTerms = Token * CandidateTerms

let private (|EmptySeq|_|) a = if Seq.isEmpty a then Some() else None

let private (|Regex|_|) pattern input =
    let res =
        Regex.Matches(input, pattern).OfType<Match>().Select(fun m -> m.Value)
        |> Seq.cast<string>

    match res with
    | EmptySeq -> None
    | _ -> res |> Some

let private Default: CandidateTerms = Seq.empty<CandidateTerm>
let private TokenSplitterRegex = "([A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9]+|[a-z]+)"

let candidateTermsFromToken =
    function
    | None -> Default
    | Some(Regex TokenSplitterRegex tokens) -> tokens
    | Some(token) -> seq { token }

let private chunkLengths tokenList = seq { 1 .. (Seq.length tokenList) }

let private chunkToTokenAndParts chunk : TokenAndCandidateTerms =
    String.concat "" chunk, Seq.cast<string> chunk

let private tokensToTokenAndParts tokens chunkSize =
    Seq.windowed chunkSize tokens |> Seq.map chunkToTokenAndParts

let private normalise tokenList : TokenAndCandidateTerms seq =
    let gatherTokenChunks = tokensToTokenAndParts tokenList
    tokenList |> chunkLengths |> Seq.collect gatherTokenChunks

let tokenToTokenAndCandidateTerms = candidateTermsFromToken >> normalise

let private termEquals (term:Definitions.Term) (candidateTerm:string) =
    let normalisedTerm = term.Name.Replace(" ", "")
    let singularEquals = normalisedTerm.Equals(candidateTerm, System.StringComparison.InvariantCultureIgnoreCase)
    let singularCandidate = candidateTerm.Singularize(false, false)
    let pluralEquals = normalisedTerm.Equals(singularCandidate, System.StringComparison.InvariantCultureIgnoreCase)
    singularEquals || pluralEquals

let private termEqualsCandidate (term:Definitions.Term) (tokenAndCandidateTerms:TokenAndCandidateTerms) = fst tokenAndCandidateTerms |> termEquals term
let private termInCandidates (term:Definitions.Term) (tokenAndCandidateTerms:TokenAndCandidateTerms) = (snd tokenAndCandidateTerms) |> Seq.exists (termEquals term)

let termFilterForCandidateTerms tokenAndCandidateTerms =
    fun (t:Definitions.Term) -> 
        let candidateMatchesTerm = termEqualsCandidate t
        tokenAndCandidateTerms |> Seq.exists candidateMatchesTerm

let filterRelevantTerms (terms: Definitions.Term seq) (tokenAndCandidateTerms: TokenAndCandidateTerms seq) =
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


