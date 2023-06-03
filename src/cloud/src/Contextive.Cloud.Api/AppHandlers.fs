module AppHandlers

open System
open Microsoft.Extensions.Logging
open Giraffe
open Contextive.Cloud.Api

let webApp:HttpHandler =
    choose [
        Definitions.routes
        setStatusCode 404 >=> text "Not Found"
    ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

