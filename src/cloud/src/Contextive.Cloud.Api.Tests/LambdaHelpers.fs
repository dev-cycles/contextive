module Contextive.Cloud.Api.Tests.LambdaHelpers
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.TestUtilities
open Setup

let requestFactory method path body =
    APIGatewayHttpApiV2ProxyRequest(
        RequestContext = APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext(
            Http = APIGatewayHttpApiV2ProxyRequest.HttpDescription(
                Method = method,
                Path = path
            ),
            DomainName = "localhost"
        ),
        Body = Option.defaultValue null body
    )

let lambdaRequest (lambdaFunction:LambdaEntryPoint) method path body = async {
    let context = TestLambdaContext()
    let request = requestFactory method path body
    return! lambdaFunction.FunctionHandlerAsync(request, context) |> Async.AwaitTask
}