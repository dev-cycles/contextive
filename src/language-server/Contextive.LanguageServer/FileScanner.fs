module Contextive.LanguageServer.FileScanner

open Microsoft.Extensions.FileSystemGlobbing
open Microsoft.Extensions.FileSystemGlobbing.Abstractions
open System.IO

let private fileScanPath glob (basePath: string) =
    let matcher = Matcher()

    matcher
        .AddInclude(glob)
        .Execute(DirectoryInfoWrapper(DirectoryInfo(basePath)))
        .Files
    |> Seq.map (fun m -> Path.Combine(basePath, m.Path))

let fileScanner (basePaths: string seq) glob =
    basePaths |> Seq.collect (fileScanPath glob)
