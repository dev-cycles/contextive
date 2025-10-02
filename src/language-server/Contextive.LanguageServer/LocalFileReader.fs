module Contextive.LanguageServer.LocalFileReader

open System.IO
open Contextive.Core.File

let read (path: PathConfiguration) =
    if File.Exists path.Path then
        File.ReadAllText path.Path |> Ok
    else
        FileNotFound path.Source |> Error
