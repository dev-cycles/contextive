module Contextive.LanguageServer.FileScanner

open Microsoft.Extensions.FileSystemGlobbing
open Microsoft.Extensions.FileSystemGlobbing.Abstractions
open System.IO

let fileScanner basePath () =
    let matcher = Matcher()

    matcher
        .AddInclude("**/*.contextive.yml")
        .Execute(DirectoryInfoWrapper(DirectoryInfo(basePath)))
        .Files
    |> Seq.map (fun m -> Path.Combine(basePath, m.Path))
