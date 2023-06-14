module Contextive.Cloud.Api.DefinitionsRepository

open System
open Amazon.S3
open Amazon.S3.Model
open AwsHelpers

let bucketName() = System.Environment.GetEnvironmentVariable("DEFINITIONS_BUCKET_NAME")

let saveDefinitions slug yml = task {
    use s3Client = new AmazonS3Client(
        AmazonS3Config(ForcePathStyle=true) |> forEnvironment
    )
    let! _ = s3Client.PutObjectAsync(PutObjectRequest(
        BucketName=bucketName(),
        Key=slug,
        ContentBody=yml
    ), Threading.CancellationToken.None)
    return Ok(yml)
}

let getDefinitions slug = task {
    use s3Client = new AmazonS3Client(
        AmazonS3Config(ForcePathStyle=true) |> forEnvironment
    )
    use! response = s3Client.GetObjectAsync(GetObjectRequest(
        BucketName=bucketName(),
        Key=slug
    ), Threading.CancellationToken.None)
    use reader = new IO.StreamReader(response.ResponseStream, Text.Encoding.UTF8)
    let content = reader.ReadToEnd()
    return Ok(content)
}