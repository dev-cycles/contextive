module Contextive.Cloud.Api.Tests.DefinitionsHandlerTests

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.TestUtilities
open Expecto
open Swensen.Unquote
open Setup

let requestFactory method path body =
    APIGatewayHttpApiV2ProxyRequest(
        RequestContext = APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext(
            Http = APIGatewayHttpApiV2ProxyRequest.HttpDescription(
                Method = method,
                Path = path
            )
        ),
        Body = body
    )

let lambdaRequest (lambdaFunction:LambdaEntryPoint) method path body = async {
    let context = TestLambdaContext()
    let request = requestFactory method path body
    return! lambdaFunction.FunctionHandlerAsync(request, context) |> Async.AwaitTask
}

let simpleDefinitions = """
contexts:
    - terms:
        - name: simpleName
"""

let invalidDefinitions = """
contexts: :
    - term:
    - name: wrong schema
"""

[<Tests>]
let definitionsHandlerTests = 
    testList "Cloud.Api.DefinitionsHandler" [

        testAsync "Can PUT a valid definitions File" {
            let lambdaFunction = LambdaEntryPoint()
            let! response = lambdaRequest lambdaFunction "PUT" "/definitions/someSlug" simpleDefinitions
            test <@ response.StatusCode = 200 @>
            test <@ response.Body = simpleDefinitions @>
        }

        testAsync "Can't PUT an invalid definitions File" {
            let lambdaFunction = LambdaEntryPoint()
            let! response = lambdaRequest lambdaFunction "PUT" "/definitions/someSlug" invalidDefinitions
            test <@ response.StatusCode = 400 @>
            test <@ response.Body = "Error parsing definitions file:  Object starting line 2, column 11 - Mapping values are not allowed in this context." @>
        }
    ]