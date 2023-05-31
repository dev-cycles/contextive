module AppHandlers

open System
open Microsoft.Extensions.Logging
open Giraffe
open Microsoft.AspNetCore.Http

let indexHandlerWithArg (arg:string) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        text $"Serverless Giraffe Web API is at {ctx.Request.Path} with arg {arg}" next ctx

let indexHandler = indexHandlerWithArg "N/A"

let arrayExampleHandler (itemCount:int) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let values = seq { for a in 1 .. itemCount do yield sprintf "value%i" a }
        text (String.concat ", " values) next ctx

let webApp:HttpHandler =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler
                route "/definitions" >=> indexHandler
                routef "/definitions/%s" indexHandlerWithArg
                route "/array" >=> arrayExampleHandler 2
                routef "/array/%i" arrayExampleHandler
            ]
        PUT >=>
            choose [
                routef "/definitions/%s" indexHandlerWithArg
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

