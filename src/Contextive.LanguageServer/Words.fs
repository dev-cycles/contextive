module Contextive.LanguageServer.Words

open System.Text.RegularExpressions
open System.Linq

let private (|EmptySeq|_|) a = if Seq.isEmpty a then Some () else None

let private (|Regex|_|) pattern input =
    let res = Regex.Matches(input, pattern)
                .OfType<Match>()
                .Select(fun m -> m.Value)
                |> Seq.cast<string>
    match res with
    | EmptySeq -> None
    | _ -> res |> Some

let private Default = Seq.empty<string>
let private WordSplitterRegex = "([A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9]+|[a-z]+)"

let private splitIntoParts =
    function | None -> Default
             | Some(Regex WordSplitterRegex words) -> words
             | Some(w) -> seq {w}

type WordAndParts = string * seq<string>

let private combine wordList : WordAndParts seq =
    seq { 1 .. (Seq.length wordList) }
    |> Seq.collect (fun chunkSize -> 
        Seq.windowed chunkSize wordList
        |> Seq.map (fun chunk -> (String.concat "" chunk, Seq.cast<string> chunk) ))

let split = splitIntoParts >> combine



