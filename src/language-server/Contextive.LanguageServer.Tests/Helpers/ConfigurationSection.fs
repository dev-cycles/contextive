module Contextive.LanguageServer.Tests.Helpers.ConfigurationSection

open Newtonsoft.Json.Linq
open System.Linq
open System.Collections.Generic
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open System.Threading.Tasks

let jTokenFromMap values = 
    let configValue = JObject()
    values |> Map.iter (fun k (v:obj) ->
        configValue.[k] <- 
            match v with
            | :? JToken as jv -> jv
            | _ -> JValue(v))
    configValue

let private configSectionResultFromMap values =
    let results = new List<JToken>()
    results.Add(jTokenFromMap values)
    Container(results)

let private includesSection section (configRequest:ConfigurationParams) =
    configRequest.Items |> Seq.map (fun ci -> ci.Section) |> Seq.contains section

let private createHandler section configValuesLoader =
    fun (configRequest:ConfigurationParams) ->
        let configSectionResult = configSectionResultFromMap <| configValuesLoader()
        if configRequest.Items.Count() = 0 || configRequest |> includesSection section then
            Task.FromResult(configSectionResult)
        else
            Task.FromResult(configSectionResultFromMap <| Map[])

let optionsBuilder section configValuesLoader (b:LanguageClientOptions) =
    let handler = createHandler section configValuesLoader
    b.WithCapability(Capabilities.DidChangeConfigurationCapability(DynamicRegistration=true))
        .OnConfiguration(handler)
        |> ignore
    b

type PathLoader = unit -> obj

let mapLoader (pathLoader:PathLoader) () = Map[("path", pathLoader())]

let contextivePathLoaderOptionsBuilder (pathLoader:PathLoader) = optionsBuilder "contextive" <| mapLoader pathLoader

let contextivePathOptionsBuilder path = contextivePathLoaderOptionsBuilder (fun () -> path)

let didChange (client:ILanguageClient) path =
    let setting = jTokenFromMap <| Map[("path", path)]
    let configSection = jTokenFromMap <| Map[("contextive", setting)]

    let didChangeConfig = DidChangeConfigurationParams(
        Settings = configSection
    )
    client.Workspace.DidChangeConfiguration(didChangeConfig)