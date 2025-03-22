module Contextive.LanguageServer.LocalFileReader

open System.IO
open Contextive.Core.File

let read (path: string) =
    if File.Exists path then
        File.ReadAllText path |> Ok
    else
        Error FileNotFound
