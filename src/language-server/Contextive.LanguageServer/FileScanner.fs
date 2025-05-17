module Contextive.LanguageServer.FileScanner

open Microsoft.Extensions.FileSystemGlobbing
open Microsoft.Extensions.FileSystemGlobbing.Abstractions
open System.IO

let private fileScanPath (globs: string array) (basePath: string) =
    let matcher = Matcher()

    matcher.AddIncludePatterns globs

    matcher.Execute(DirectoryInfoWrapper(DirectoryInfo(basePath))).Files
    |> Seq.map (fun m -> Path.Combine(basePath, m.Path))

let fileScanner (basePaths: string seq) glob =
    basePaths |> Seq.collect (fileScanPath glob)
