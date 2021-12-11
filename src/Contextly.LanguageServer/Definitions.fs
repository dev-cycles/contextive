module Contextly.LanguageServer.Definitions

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open System.IO
open System.Collections.Concurrent

[<CLIMutable>]
type Term =
    {
        Name: string
        Definition: string option
        Examples: ResizeArray<string>
    }
    static member Default = {Name = ""; Definition = None; Examples = null}

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
type Loader = string option -> string option -> string option 
type Reloader = unit -> string option

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

let private loadContextly path =
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

let load (instanceId:string) : Loader = fun (workspaceFolder:string option) (path:string option) ->
    let absolutePath = getPath workspaceFolder path
    
    match absolutePath with
    | Some ap ->
        match loadContextly ap with
        | Some(defs) ->
            definitions.AddOrUpdate(instanceId, defs, fun _ _ -> defs) |> ignore
        | None -> ()
    | None -> ()
    
    absolutePath

let find (instanceId:string) : Finder = fun filter -> 
    let mutable defs : Definitions = { Contexts = new ResizeArray<Context>() }

    match definitions.TryGetValue(instanceId, &defs) with
    | false -> seq []
    | true -> defs.Contexts |> Seq.collect(fun c -> c.Terms) |> Seq.filter filter
    

    