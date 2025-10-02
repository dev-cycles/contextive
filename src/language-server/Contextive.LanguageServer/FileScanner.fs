module Contextive.LanguageServer.FileScanner

open Microsoft.Extensions.FileSystemGlobbing
open Microsoft.Extensions.FileSystemGlobbing.Abstractions
open System.IO

let private loadGitIgnore (basePath: string) (gitIgnorePath: string) =
    let gitIgnoreBasePath =
        Path.GetRelativePath(basePath, Path.GetDirectoryName gitIgnorePath)

    if File.Exists gitIgnorePath then
        File.ReadAllLines gitIgnorePath
        |> Seq.collect (fun ignorePattern ->
            [ $"{gitIgnoreBasePath}/**/{ignorePattern}"
              $"{gitIgnoreBasePath}/{ignorePattern}" ])
    else
        [||]

let fileScanPathsWithIgnore (globs: string seq) (basePath: string) (ignoreList: string seq) =
    let matcher = Matcher()

    matcher.AddIncludePatterns globs

    matcher.AddExcludePatterns ignoreList

    matcher.Execute(DirectoryInfoWrapper(DirectoryInfo basePath)).Files
    |> Seq.map (fun m -> Path.Combine(basePath, m.Path))

let private fileScanPath (globs: string array) (basePath: string) =
    let excludePatterns = Path.Combine(basePath, ".gitignore") |> loadGitIgnore basePath

    let gitIgnoreFiles =
        fileScanPathsWithIgnore [| """**/\.gitignore""" |] basePath excludePatterns

    let excludePatterns =
        gitIgnoreFiles
        |> Seq.collect (loadGitIgnore basePath)
        |> Seq.append excludePatterns
        |> Array.ofSeq

    fileScanPathsWithIgnore globs basePath excludePatterns

let fileScanner (basePaths: string seq) glob =
    basePaths |> Seq.collect (fileScanPath glob)
