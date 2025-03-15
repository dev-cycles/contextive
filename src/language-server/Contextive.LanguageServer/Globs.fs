module Contextive.LanguageServer.Globs

open DotNet.Globbing
open Contextive.Core.GlossaryFile

let matchPreciseGlob (openFileUri: string) pathGlob =
    Glob.Parse(pathGlob).IsMatch(openFileUri)

let matchGlob (openFileUri: string) pathGlob =
    let formats: Printf.StringFormat<string -> string> list =
        [ "%s"; "**%s"; "**%s*"; "**%s**" ]

    formats
    |> List.map (fun s -> sprintf s pathGlob)
    |> List.exists (matchPreciseGlob openFileUri)

let matchGlobs openFileUri (context: Context) =
    let matchOpenFileUri = matchGlob openFileUri

    match context.Paths with
    | null -> true
    | _ -> context.Paths |> Seq.exists matchOpenFileUri
