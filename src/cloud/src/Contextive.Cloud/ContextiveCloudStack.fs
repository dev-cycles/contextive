namespace Contextive.Cloud

open System
open Amazon.CDK
open Amazon.CDK.AWS.S3
open Amazon.CDK.AWS.Lambda
open Amazon.CDK.AWS.Logs
open Amazon.CDK.AWS.Apigatewayv2
open Amazon.CDK.AWS.Apigatewayv2.Alpha
open Amazon.CDK.AWS.Apigatewayv2.Integrations.Alpha


type ContextiveCloudStack(scope, id, props) as this =
    inherit Stack(scope, id, props)

    let assemblyPath relativePath = 
        let basePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        System.IO.Path.Combine(basePath, relativePath)

    let definitions = Bucket(this, "Definitions", BucketProps())

    let apiFunctionProps =
        FunctionProps(Runtime = Runtime.DOTNET_6,
                      Code = Code.FromAsset(assemblyPath "../../../../Contextive.Cloud.Api/bin/Debug/net6.0/linux-x64/publish"),
                      Handler = "Contextive.Cloud.Api::Setup+LambdaEntryPoint::FunctionHandlerAsync",
                      Description = "Contextive Api",
                      MemorySize = Nullable<float>(256.0),
                      Environment = Map [ "DEFINITIONS_BUCKET_NAME", definitions.BucketName ],
                      LogRetention = Option.toNullable (Some RetentionDays.ONE_WEEK))

    do CfnOutput(this, "DefinitionsBucketName", CfnOutputProps(Value=definitions.BucketName)) |> ignore

    let apiFunction = Function(this, "Api", apiFunctionProps)

    let isLocal = this.Node.TryGetContext("local")
    do printfn "isLocal: %A" isLocal
    do match isLocal with
        | null -> 
            printfn "matched null"
            let apiIntegration = HttpLambdaIntegration("ApiIntegration", apiFunction)
            printfn "Adding httpApi"
            let httpApi = HttpApi(this, "HttpApi");

            httpApi.AddRoutes(AddRoutesOptions(
                Path = "/",
                Methods = [| HttpMethod.ANY |],
                Integration = apiIntegration
            )) |> ignore
            httpApi.AddRoutes(AddRoutesOptions(
                Path = "/{proxy+}",
                Methods = [| HttpMethod.ANY |],
                Integration = apiIntegration
            )) |> ignore

            CfnOutput(this, "HttpApiEndpoint", CfnOutputProps(Value=httpApi.Url)) |> ignore
        | _ -> 
            printfn "didn't match null"
            printfn "Not adding http API"
            ()
        

