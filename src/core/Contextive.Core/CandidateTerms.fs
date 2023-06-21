module Contextive.Core.CandidateTerms

open System.Text.RegularExpressions
open System.Linq

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
