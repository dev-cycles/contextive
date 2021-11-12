module Ubictionary.LanguageServer.Tests.ConfigurationSection

open Newtonsoft.Json.Linq
open System.Collections.Generic
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open System.Threading.Tasks

let private fromMap values =
    let results = new List<JToken>()
    let configValue = JObject()
    results.Add(configValue)
    values |> Map.iter (fun k (v:string) ->
        configValue.[k] <- JValue(v))
    Container(results)

let private includesSection section (configRequest:ConfigurationParams) =
    configRequest.Items |> Seq.map (fun ci -> ci.Section) |> Seq.contains section

let private createHandler section configValues =
    let configSectionResult = fromMap configValues
    fun c ->
        if includesSection section c then
            Task.FromResult(configSectionResult)
        else
            Task.FromResult(null)

let optionsBuilder section configValues (b:LanguageClientOptions) =
    let handler = createHandler section configValues
    b.WithCapability(Capabilities.DidChangeConfigurationCapability())
        .OnConfiguration(handler)
        |> ignore
    b