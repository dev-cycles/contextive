#r "nuget: CrypticWizard.RandomWordGenerator"
#r @"../../../../core/Contextive.Core/obj/Debug/net8.0/Contextive.Core.dll"
#r @"nuget: YamlDotNet"

open Contextive.Core.GlossaryFile
open CrypticWizard.RandomWordGenerator

/// <summary>
/// <para>This script can be run with `dotnet fsi generate_perf_glossaries.fsx`</para>
/// <para>It will generate a set of glossary files with random term names and definitions.</para>
/// <para>The number of terms in each file is set by the constant value `numTermsInFile`.<para>
/// <para>The number of files generated is set by the constant value `numFiles`.</para>
/// </summary>
/// <remarks>
/// <para>If the files are regenerated, be sure to update the example terms in:</para>
/// <para>the HoverTests.fs file in the `Hover is within performance limit with many very large glossaries` test and
/// <para>the example term in CompletionTests.fs in the `Test Completions with large glossaries` test.</para>
/// </remarks>

[<Literal>]
let numTermsInFile = 10_000

[<Literal>]
let numFiles = 8

let wg = WordGenerator()

// https://github.com/TheAlgorithms/F-Sharp/blob/main/Algorithms/Strings/Capitalize.fs
let capitalize (sentence: string) =
    match sentence with
    | "" -> ""
    | s when s.[0] >= 'a' && s.[0] <= 'z' -> sentence.Remove(0, 1).Insert(0, (string) ((char) ((int) s.[0] - 32)))
    | _ -> sentence

let rec getWords len =
    try
        wg.GetWords len
    with _ ->
        getWords len

let newSentence len =
    wg.SetSeed(System.DateTime.Now.Ticks |> int)
    System.String.Join(" ", getWords len) |> capitalize

let newPara numSentences sentenceLen =
    let sentences =
        seq { 1..numSentences } |> Seq.map (fun _ -> newSentence sentenceLen + ".")

    System.String.Join(System.Environment.NewLine + System.Environment.NewLine, sentences)

let makeTermList (numTerms: int) =
    seq { 0..numTerms }
    |> Seq.map (fun idx ->
        { Term.Default with
            Name = newSentence 3
            Definition = newPara 5 8 |> Some })

let makeContext (numTerms: int) =
    let terms = makeTermList numTerms

    { Context.Default with
        Name = ""
        DomainVisionStatement = ""
        Terms = terms |> ResizeArray }

let makeFile (numTerms: int) (fileIdx: int) =

    printfn "fileIdx: %A" fileIdx

    let context1 = makeContext numTerms
    let context2 = makeContext numTerms

    let gf =
        { GlossaryFile.Default with
            Contexts = [| context1; context2 |] |> ResizeArray }

    System.IO.File.WriteAllText($"perf{fileIdx}.glossary.yml", serialize gf)

seq { 1..numFiles } |> List.ofSeq |> List.map (makeFile numTermsInFile)
