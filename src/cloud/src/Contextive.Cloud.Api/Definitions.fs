module Contextive.Cloud.Api.Definitions

open System
open Microsoft.Extensions.Logging
open Giraffe
open Microsoft.AspNetCore.Http
open Contextive.Core.Definitions

let clientError msg = setStatusCode 400 >=> setBodyFromString msg

let put (slug:string) =
    fun (next : HttpFunc) (ctx : HttpContext) -> task {
        let! yml = ctx.ReadBodyFromRequestAsync()
        let definitions = deserialize yml
        return! 
            match definitions with
            | Ok(defs) -> text yml next ctx
            | Error(message) -> 
                (clientError message) next ctx
    }
