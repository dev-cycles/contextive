module Contextive.Cloud.HttpApi

open Amazon.CDK
open Amazon.CDK.AWS.Apigatewayv2
open Amazon.CDK.AwsApigatewayv2Integrations

let setup construct apiFunction =
    let apiIntegration = HttpLambdaIntegration("ApiIntegration", apiFunction)
    let httpApi = HttpApi(construct, "HttpApi")

    httpApi.AddRoutes(AddRoutesOptions(Path = "/", Methods = [| HttpMethod.ANY |], Integration = apiIntegration))
    |> ignore

    httpApi.AddRoutes(
        AddRoutesOptions(Path = "/{proxy+}", Methods = [| HttpMethod.ANY |], Integration = apiIntegration)
    )
    |> ignore

    CfnOutput(construct, "HttpApiEndpoint", CfnOutputProps(Value = httpApi.Url))
    |> ignore
