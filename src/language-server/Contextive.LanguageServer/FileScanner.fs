module Contextive.LanguageServer.FileScanner

open Microsoft.Extensions.FileSystemGlobbing
open Microsoft.Extensions.FileSystemGlobbing.Abstractions
open System.IO
open System

let private stripComments (ignorePattern: string) =
    ignorePattern.Split "#" |> Seq.head |> _.Trim()

let private matchAnywhereInSubFolders (gitIgnoreBasePath: string) (ignorePattern: string) =
    $"{gitIgnoreBasePath}/**/{ignorePattern}"

let private loadGitIgnore (basePath: string) (gitIgnorePath: string) =
    let gitIgnoreBasePath =
        Path.GetRelativePath(basePath, Path.GetDirectoryName gitIgnorePath)

    if File.Exists gitIgnorePath then
        File.ReadAllLines gitIgnorePath
        |> Seq.map stripComments
        |> Seq.filter (not << String.IsNullOrEmpty)
        |> Seq.map (matchAnywhereInSubFolders gitIgnoreBasePath)
    else
        [||]

let fileScanPathsWithIgnore (globs: string seq) (basePath: string) (ignoreList: string seq) =
    let matcher = Matcher()

    matcher.AddIncludePatterns globs

    matcher.AddExcludePatterns ignoreList

    matcher.Execute(DirectoryInfoWrapper(DirectoryInfo basePath)).Files
    |> Seq.map (fun m -> Path.Combine(basePath, m.Path))

let private fileScanPath (logger: string -> unit) (globs: string array) (basePath: string) =
    let excludePatterns = Path.Combine(basePath, ".gitignore") |> loadGitIgnore basePath

    logger $"""Found initial exclude patterns in the root .gitignore {sprintf "%A" (List.ofSeq excludePatterns)}"""

    let gitIgnoreFiles =
        fileScanPathsWithIgnore [| """**/\.gitignore""" |] basePath excludePatterns

    logger $"""Found more gitIgnore files: {sprintf "%A" (List.ofSeq gitIgnoreFiles)}"""

    let excludePatterns =
        gitIgnoreFiles
        |> Seq.collect (loadGitIgnore basePath)
        |> Seq.append excludePatterns
        |> Array.ofSeq

    logger $"""Full list of exclude patterns from all gitIgnores {sprintf "%A" (List.ofSeq excludePatterns)}"""

    logger "Starting scan..."
    let files = fileScanPathsWithIgnore globs basePath excludePatterns
    logger $"""Found files {sprintf "%A" (List.ofSeq files)}"""
    files

let fileScanner (logger: string -> unit) (basePaths: string seq) glob =
    basePaths |> Seq.collect (fileScanPath logger glob)
