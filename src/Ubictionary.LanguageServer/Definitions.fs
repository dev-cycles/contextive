module Ubictionary.LanguageServer.Definitions

open Legivel.Serialization
open System.IO

type Term =
    {
        Name: string
    }

type Context =
    {
        Name: string
        Paths: string list
        Terms: Term list
    }

type Definitions =
    {
        ProjectName: string
        Description: string
        Contexts: Context list
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
        | Some d -> d.Contexts |> Seq.collect(fun c -> c.Terms)
    terms |> Seq.map transformer