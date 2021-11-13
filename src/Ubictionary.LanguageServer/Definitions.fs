module Ubictionary.LanguageServer.Definitions

open Legivel.Serialization
open System.IO

type Term =
    {
        name: string
    }

type Context =
    {
        name: string
        paths: string list
        terms: Term list
    }

type Definitions =
    {
        projectName: string
        description: string
        contexts: Context list
    }


let mutable private ubictionaryPath : string option = None
let mutable definitions : Definitions option = None

let private tryReadFile path =
    try 
      File.ReadAllText(path)
    with
    | _ -> ""

let loadUbictionary path =
    let yml = tryReadFile path
    match Deserialize yml with
    | [Success r] -> Some r.Data
    | _ -> None

let clear () =
    definitions <- None

let load (path:string) =
    ubictionaryPath <- Some path
    definitions <- loadUbictionary path
    ()

let find (matcher: Term -> bool) (transformer: Term -> 'a) : 'a seq =
    let terms =
        match definitions  with
        | None -> seq []
        | Some d -> d.contexts |> Seq.collect(fun c -> c.terms)
    terms |> Seq.map transformer