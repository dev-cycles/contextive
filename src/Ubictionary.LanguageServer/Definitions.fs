module Ubictionary.LanguageServer.Definitions

open Legivel.Serialization
open System.IO
open System.Collections.Concurrent

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

type Finder = (Term -> bool) -> (Term -> string) -> string seq

let mutable private definitions : ConcurrentDictionary<string, Definitions> = new ConcurrentDictionary<string, Definitions>()

let private tryReadFile path =
    if File.Exists(path) then
      File.ReadAllText(path) |> Some
    else
      None

let private deserialize yml =
    try
        match Deserialize yml with
        | [Success r] -> Some r.Data
        | e -> 
            printfn "%A" e
            None
    with
    | e -> 
        printfn "%A" e
        None

let private loadUbictionary path =
    match tryReadFile path with
    | None -> None
    | Some(ymlText) -> deserialize ymlText

let private getPath workspaceFolder (path: string option) =
    match path with
    | None -> None
    | Some p -> 
        if Path.IsPathRooted(p) then
            path
        else match workspaceFolder with
             | Some wsf -> Path.Combine(wsf, p) |> Some
             | None -> None

let load (id:string) (workspaceFolder:string option) (path:string option) =
    let absolutePath = getPath workspaceFolder path
    
    match absolutePath with
    | Some ap ->
        match loadUbictionary ap with
        | Some(defs) -> definitions.TryAdd(id, defs) |> ignore
        | None -> ()
    | None -> ()
    
    absolutePath

let find (id:string) (matcher: Term -> bool) (transformer: Term -> 'a) : 'a seq =
    
    let mutable defs : Definitions = { contexts = [] }

    let terms =
        match definitions.TryGetValue(id, &defs) with
        | false -> seq []
        | true -> defs.contexts |> Seq.collect(fun c -> c.terms)
    terms |> Seq.map transformer