module Contextive.Cloud.Api.Definitions

open Giraffe
open Microsoft.AspNetCore.Http
open Contextive.Core.Definitions
open DefinitionsRepository

let clientError msg =
    setStatusCode 400 >=> setBodyFromString msg

let put (slug: string) =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! yml = ctx.ReadBodyFromRequestAsync()
            let definitions = deserialize yml

            return!
                match definitions with
                | Ok(_) ->
                    task {
                        let! saved = saveDefinitions slug yml

                        return!
                            match saved with
                            | Ok(s) -> text s next ctx
                            | Error(msg) -> (setStatusCode 500 >=> setBodyFromString msg) next ctx
                    }
                | Error(message) -> (clientError message) next ctx
        }

let get (slug: string) =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! yml = getDefinitions slug

            return!
                match yml with
                | Ok(y) -> text y next ctx // lastModified
                | Error(msg) -> (setStatusCode 500 >=> setBodyFromString msg) next ctx
        }


let routes: HttpHandler =
    choose [ PUT >=> routef "/definitions/%s" put; GET >=> routef "/definitions/%s" get ]
