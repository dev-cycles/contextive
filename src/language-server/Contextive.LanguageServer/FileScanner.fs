module Contextive.LanguageServer.FileScanner

open Microsoft.Extensions.FileSystemGlobbing
open Microsoft.Extensions.FileSystemGlobbing.Abstractions
open System.IO

let fileScanner basePath glob =
    let matcher = Matcher()

    matcher
        .AddInclude(glob)
        .Execute(DirectoryInfoWrapper(DirectoryInfo(basePath)))
        .Files
    |> Seq.map (fun m -> Path.Combine(basePath, m.Path))
