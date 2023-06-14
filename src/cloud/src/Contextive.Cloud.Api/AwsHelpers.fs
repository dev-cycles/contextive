module Contextive.Cloud.Api.AwsHelpers

open Amazon.Runtime

let isLocal = System.Environment.GetEnvironmentVariable("IS_LOCAL") = "True"
let awsEndpoint() = if isLocal then "http://localstack:4566" else null

let forEnvironment<'T when 'T :> ClientConfig> (config:'T) : 'T =
    if isLocal then
        config.ServiceURL <- awsEndpoint()
    config