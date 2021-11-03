module Ubictionary.LanguageServer.Tests.ConfigurationSection

open Newtonsoft.Json.Linq
open System.Collections.Generic
open OmniSharp.Extensions.LanguageServer.Protocol.Models

let fromMap values =
    let results = new List<JToken>()
    let configValue = JObject()
    results.Add(configValue)
    values |> Map.iter (fun k (v:string) ->
        configValue.[k] <- JValue(v))
    Container(results)

let includesSection section (configRequest:ConfigurationParams) =
    configRequest.Items |> Seq.map (fun ci -> ci.Section) |> Seq.contains section