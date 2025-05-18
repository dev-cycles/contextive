module Contextive.Core.Normalization

open Humanizer
open System.Text.RegularExpressions

// \\p{M} is the regex to match any codepoint that is a combining mark.  Since the string has already been normalised with FormKD,
// the diacritic is split into a separate char in the utf-16 string and can be independently removed.
let private utfNormalized s = Regex.Replace(s, "\\p{M}", "")

// \u0308 is the unicode point for the two-dots (https://codepoints.net/U+0308) Since the string has already been normalised with FormKD,
// the diacritic is split into a sparate char in the utf-16 string and can be independently replaced.
// ß is an independent character and not a combining codepoint, so must be explicitly replaced.
let germanNormalised (s: string) =
    s.Replace("\u0308", "e").Replace("ß", "ss")

let simpleNormalize (term: string) : string =
    term
        .ToLower()
        .Singularize(false, false)
        .Replace(" ", "")
        .Replace("_", "")
        .Replace("-", "")
        .Normalize
        System.Text.NormalizationForm.FormKD

let normalize (term: string) : string seq =
    let normalizedTerm = simpleNormalize term

    Set(
        seq {
            normalizedTerm
            utfNormalized normalizedTerm
            germanNormalised normalizedTerm
        }
    )
    |> Set.toSeq
