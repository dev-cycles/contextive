module Contextive.LanguageServer.Tests.Helpers.ConfigurationSection

open Newtonsoft.Json.Linq
open System.Linq
open System.Collections.Generic
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Client
open OmniSharp.Extensions.LanguageServer.Protocol.Workspace
open System.Threading.Tasks

let private jTokenFromMap values =
    let configValue = JObject()

    values
    |> Map.iter (fun k (v: obj) ->
        configValue.[k] <-
            match v with
            | :? JToken as jv -> jv
            | _ -> JValue(v))

    configValue

let private configSectionResultFromMap values =
    let results = new List<JToken>()
    results.Add(jTokenFromMap values)
    Container(results)

let private includesSection section (configRequest: ConfigurationParams) =
    configRequest.Items |> Seq.map (fun ci -> ci.Section) |> Seq.contains section

let private createHandler section configValuesLoader =
    fun (configRequest: ConfigurationParams) ->
        let configSectionResult = configSectionResultFromMap <| configValuesLoader ()

        if configRequest.Items.Count() = 0 || configRequest |> includesSection section then
            Task.FromResult(configSectionResult)
        else
            Task.FromResult(configSectionResultFromMap <| Map [])

let configurationHandlerBuilder section configValuesLoader (b: LanguageClientOptions) =
    let handler = createHandler section configValuesLoader
    b.OnConfiguration(handler) |> ignore
    b

let private didChangeConfigurationBuilder (b: LanguageClientOptions) =
    b.WithCapability(Capabilities.DidChangeConfigurationCapability(DynamicRegistration = true))
    |> ignore

    b

type ValueLoader = unit -> obj

let private mapLoader (key: string) (valueLoader: ValueLoader) () = Map[(key, valueLoader ())]

let private mapPathLoader = mapLoader "path"

let private contextivePathConfigurationHandlerBuilder (pathLoader: ValueLoader) =
    configurationHandlerBuilder "contextive" <| mapPathLoader pathLoader

/// <summary>
///     <para>Use when you have a setting value that needs to change during the test.</para>
///     <para>The test client will support `workspace/configuration` AND `workspace/didChangeConfiguration`.</para>
///     <para>When a `workspace/configuration` request is received, the `pathLoader` function will be invoked to get the current value.</para>
///     <para>To trigger a `workspace/didChangeConfiguration` notification, use <see cref="didChangePath">didChangePath</see>.</para>
/// </summary>
let contextivePathLoaderBuilder (pathLoader: ValueLoader) =
    contextivePathConfigurationHandlerBuilder pathLoader
    >> didChangeConfigurationBuilder

/// <summary>
///     <para>Use when you have a fixed setting value.</para>
///     <para>The test client will ONLY support `workspace/configuration` and will always supply this fixed value</para>
///     <para>Using <see cref="didChangePath">didChangePath</see> will have no impact as the server will not support the `workspace/didChangeConfiguration` notification</para>
/// </summary>
let contextivePathBuilder path =
    contextivePathConfigurationHandlerBuilder (fun () -> path)

/// <summary>
///     <para>Trigger a `workspace/didChangeConfiguration` notification with the new value.</para>
///     <para>This will only work if the TestClient was created with the <see cref="contextivePathLoaderBuilder">contextivePathLoaderBuilder</see>.</para>
///     <para>Ensure this method is invoked with the same path value as will be returned from the `pathLoader` registered with the TestClient.</para>
/// </summary>
let didChangePath (client: ILanguageClient) path (logAwaiter: ConditionAwaiter.Awaiter<string> option) =
    async {
        let setting = jTokenFromMap <| Map[("path", path)]
        let configSection = jTokenFromMap <| Map[("contextive", setting)]

        let didChangeConfig = DidChangeConfigurationParams(Settings = configSection)
        client.Workspace.DidChangeConfiguration(didChangeConfig)

        let! reply =
            match logAwaiter with
            | None -> async { return None }
            | Some la -> ServerLog.waitForLogMessage la "Loading contextive"

        if Option.isNone reply then
            failwith "Server never loaded configuration after changing the path"
        else
            Serilog.Log.Logger.Debug(sprintf "Server did load contextive with: %s" reply.Value)

        match logAwaiter with
        | None -> ()
        | Some la -> ConditionAwaiter.clear la
    }
