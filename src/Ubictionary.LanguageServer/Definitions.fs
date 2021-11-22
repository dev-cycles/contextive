module Ubictionary.LanguageServer.Definitions

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.IO
open System.Collections.Concurrent

[<CLIMutable>]
type Term =
    {
        Name: string
        Definition: string option
    }

[<CLIMutable>]
type Context =
    {
        Terms: ResizeArray<Term>
    }

[<CLIMutable>]
type Definitions =
    {
        Contexts: ResizeArray<Context>
    }

type Finder = (Term -> bool) -> Term seq

let mutable private definitions : ConcurrentDictionary<string, Definitions> = new ConcurrentDictionary<string, Definitions>()

let private tryReadFile path =
    if File.Exists(path) then
      File.ReadAllText(path) |> Some
    else
      None

let private deserialize (yml:string) =
    try
        let deserializer = 
            (new DeserializerBuilder())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
        deserializer.Deserialize<Definitions>(yml) |> Some
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

let find (id:string) (filter: Term -> bool) : Term seq =
    
    let mutable defs : Definitions = { Contexts = new ResizeArray<Context>() }

    match definitions.TryGetValue(id, &defs) with
    | false -> seq []
    | true -> defs.Contexts |> Seq.collect(fun c -> c.Terms) |> Seq.filter filter
    

    