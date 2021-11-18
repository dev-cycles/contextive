module Ubictionary.LanguageServer.Definitions

open Legivel.Serialization
open System.IO

type Term =
    {
        name: string
    }

type Context =
    {
        terms: Term list
    }

type Definitions =
    {
        contexts: Context list
    }

let mutable private ubictionaryPath : string option = None
let mutable definitions : Definitions option = None

let private tryReadFile path =
    try 
      File.ReadAllText(path)
    with
    | _ -> ""

let private loadUbictionary path =
    let yml = tryReadFile path
    match Deserialize yml with
    | [Success r] -> Some r.Data
    | e -> 
        printfn "%A" e
        None

let private getPath workspaceFolder (path: string option) =
    match path with
    | None -> None
    | Some p -> 
        if Path.IsPathRooted(p) then
            path
        else match workspaceFolder with
             | Some wsf -> Path.Combine(wsf, p) |> Some
             | None -> None

let clear () =
    definitions <- None

let load (workspaceFolder:string option) (path:string option) =
    let absolutePath = getPath workspaceFolder path
    ubictionaryPath <- absolutePath
    definitions <-
        match absolutePath with
            | Some ap -> loadUbictionary ap
            | None -> None
    absolutePath

let find (matcher: Term -> bool) (transformer: Term -> 'a) : 'a seq =
    let terms =
        match definitions  with
        | None -> seq []
        | Some d -> d.contexts |> Seq.collect(fun c -> c.terms)
    terms |> Seq.map transformer