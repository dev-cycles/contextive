module Contextive.Cloud.Api.Definitions

open System
open Microsoft.Extensions.Logging
open Giraffe
open Microsoft.AspNetCore.Http
open Contextive.Core.Definitions
open Amazon.S3
open Amazon.S3.Model

let clientError msg = setStatusCode 400 >=> setBodyFromString msg

let saveDefinitions slug yml = task {
    use s3Client = new AmazonS3Client(
        AmazonS3Config(ServiceURL = "http://localstack:4566",ForcePathStyle=true)
    )
    let! _ = s3Client.PutObjectAsync(PutObjectRequest(
        BucketName="contextivecloud-definitionse678edeb-c3c59b5c",
        Key=slug,
        ContentBody=yml
    ), Threading.CancellationToken.None)
    return Ok(yml)
}

let getDefinitions slug = task {
    use s3Client = new AmazonS3Client(
        AmazonS3Config(ServiceURL = "http://localstack:4566",ForcePathStyle=true)
    )
    use! response = s3Client.GetObjectAsync(GetObjectRequest(
        BucketName="contextivecloud-definitionse678edeb-c3c59b5c",
        Key=slug
    ), Threading.CancellationToken.None)
    use reader = new IO.StreamReader(response.ResponseStream, Text.Encoding.UTF8)
    let content = reader.ReadToEnd()
    return Ok(content)
}

let put  (slug:string) =
    fun (next : HttpFunc) (ctx : HttpContext) -> task {
        let! yml = ctx.ReadBodyFromRequestAsync()
        let definitions = deserialize yml
        return! 
            match definitions with
            | Ok(_) -> task {
                let! saved = saveDefinitions slug yml
                return! match saved with
                        | Ok(s) -> text s next ctx
                        | Error(msg) -> (setStatusCode 500 >=> setBodyFromString msg) next ctx
                }
            | Error(message) -> 
                (clientError message) next ctx
    }

let get (slug:string) = 
    fun (next : HttpFunc) (ctx : HttpContext) -> task {
        let! yml = getDefinitions slug
        return! 
            match yml with
            | Ok(y) -> text y next ctx // lastModified
            | Error(msg) -> (setStatusCode 500 >=> setBodyFromString msg) next ctx
    }


let routes:HttpHandler =
    choose [
        PUT >=> routef "/definitions/%s" put
        GET >=> routef "/definitions/%s" get
    ]