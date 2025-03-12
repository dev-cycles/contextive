module Contextive.LanguageServer.GlossaryFileInitializer

module Models =

    [<CLIMutable>]
    type InitializeGlossaryFileResponse = { Success: bool }

open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.JsonRpc
open Models
open System.IO
open Contextive.Core
open Contextive.Core.File
open Contextive.Core.GlossaryFile

let private introComments =
    """# yaml-language-server: $schema=contextive-schema.json
# Welcome to Contextive!
#
# This initial glossary file illustrates the syntax of the file by providing definitions and examples of the terms
# used in schema of the glossary file.
#
# Hover over some of the defined words, such as `context`, `term`, `definition` and `example` to see Contextive in action.
#
# Update the yaml below to define your specific contexts and terms, and feel free to use markdown in definitions and examples.
"""

let private pathComments =
    """    # Globs are supported. Multiple paths may be included. If any match the currently open file, the context will be used.
"""

let private defaultGlossaryFile =
    { Contexts =
        [| { Name = "Demo"
             DomainVisionStatement = "To illustrate the usage of the contextive glossary file."
             Paths = [| "**" |] |> ResizeArray
             Terms =
               [| { Name = "context"
                    Definition = Some "A boundare within which words have specific meanings."
                    Examples =
                      [| "In the _Sales_ context, the language focuses on activities associated with selling products."
                         "Are you sure you're thinking of the definition from the right context?" |]
                      |> ResizeArray
                    Aliases = ResizeArray() }
                  { Name = "term"
                    Definition = Some "A protected word in a given context."
                    Examples =
                      [| "_Order_ is a term that is meaningful in the _Sales_ context."
                         "What term should we use to describe what happens when a customer buys something from us?" |]
                      |> ResizeArray
                    Aliases = [| "word" |] |> ResizeArray }
                  { Name = "alias"
                    Definition =
                      Some
                          "An alternative word for the same concept.
It might be a legacy term, or an alternative term that is in common use while the ubiquitous language is still being adopted."
                    Examples = [| "_Product_ is an alias of _Item_ in the Sales context." |] |> ResizeArray
                    Aliases = ResizeArray() }
                  { Name = "definition"
                    Definition = Some "A short summary defining the meaning of a term in a context."
                    Examples =
                      [| "The definition of _Order_ in the _Sales_ context is: \"The set of _Items_ that are being sold as part of this _Order_\"."
                         "Can you provide a definition of the term _Order_?" |]
                      |> ResizeArray
                    Aliases = ResizeArray() }
                  { Name = "example"
                    Definition = Some "A sentence illustrating the correct usage of the term in the context."
                    Examples =
                      [| "Can you give me an example of how to use the term _Order_ in this context?" |]
                      |> ResizeArray
                    Aliases = ResizeArray() } |]
               |> ResizeArray } |]
        |> ResizeArray }

let private insertComment (beforeText: string) comment (text: string) =
    let start = text.IndexOf(beforeText)

    text.Substring(0, start) + comment + text.Substring(start)

let private ensureDirectoryExists (p: string) =
    p |> Path.GetDirectoryName |> Directory.CreateDirectory |> ignore

let private schemaFileName = "contextive-schema.json"

let private handler pathGetter (showDocument: ShowDocumentParams -> System.Threading.Tasks.Task<ShowDocumentResult>) =
    fun () ->
        async {
            let! path = pathGetter ()

            return!
                match path with
                | Error(e) -> async { return { Success = false } }
                | Ok({ Path = p }) ->
                    async {
                        if not <| File.Exists(p) then
                            let defaultGlossaryFileText =
                                GlossaryFile.serialize defaultGlossaryFile
                                |> insertComment "contexts:" introComments
                                |> insertComment "    paths:" pathComments

                            ensureDirectoryExists p
                            File.WriteAllText(p, defaultGlossaryFileText)

                        let folder = Path.GetDirectoryName(p)
                        let schemaPath = Path.Combine(folder, schemaFileName)
                        File.WriteAllText(schemaPath, GlossaryFileSchema.schema)

                        do!
                            showDocument (ShowDocumentParams(Uri = p, External = false))
                            |> Async.AwaitTask
                            |> Async.Ignore

                        return { Success = true }
                    }
        }
        |> Async.StartAsTask

let private method = "contextive/initialize"

let private setupRequestHandler pathGetter showDocument (slr: ILanguageServerRegistry) =
    slr.OnRequest(
        method,
        System.Func<_>(handler pathGetter showDocument),
        JsonRpcHandlerOptions(RequestProcessType = RequestProcessType.Parallel)
    )
    |> ignore


let registerHandler (s: ILanguageServer) pathGetter showDocument =
    s.Register(setupRequestHandler pathGetter showDocument) |> ignore
