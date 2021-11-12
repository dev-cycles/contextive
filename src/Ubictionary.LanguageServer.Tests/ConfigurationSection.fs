module Ubictionary.LanguageServer.Tests.ConfigurationSection

open Newtonsoft.Json.Linq
open System.Collections.Generic
open OmniSharp.Extensions.LanguageServer.Protocol.Models
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

let createHandler section configValues =
    let configSectionResult = fromMap configValues
    fun c ->
        if includesSection section c then
            Task.FromResult(configSectionResult)
        else
            Task.FromResult(null)