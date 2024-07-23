module Contextive.Cloud.Api.Tests.DefinitionsHandlerTests

open Expecto
open Swensen.Unquote
open Setup
open LambdaHelpers

let simpleDefinitions =
    Some
        """contexts:
    - terms:
        - name: simpleName"""

let invalidDefinitions =
    Some
        """contexts: :
    - term:
    - name: wrong schema"""

[<Tests>]
let definitionsHandlerTests =
    testList
        "Cloud.Api.DefinitionsHandler"
        [

          testAsync "Can PUT a valid definitions File" {
              let lambdaFunction = LambdaEntryPoint()
              let! response = lambdaRequest lambdaFunction "PUT" "/definitions/someSlug1" simpleDefinitions
              test <@ (response.StatusCode, response.Body) = (200, simpleDefinitions.Value) @>
          }

          testAsync "Can't PUT an invalid definitions File" {
              let lambdaFunction = LambdaEntryPoint()
              let! response = lambdaRequest lambdaFunction "PUT" "/definitions/someSlug2" invalidDefinitions

              test
                  <@
                      (response.StatusCode, response.Body) = (400,
                                                              "ParsingError\n  \"Object starting line 1, column 11 - Mapping values are not allowed in this context.\"")
                  @>
          }

          testAsync "Can GET a definitions file that was previously PUT" {
              let lambdaFunction = LambdaEntryPoint()
              let! response = lambdaRequest lambdaFunction "PUT" "/definitions/someSlug3" simpleDefinitions
              test <@ (response.StatusCode, response.Body) = (200, simpleDefinitions.Value) @>
              let! getResponse = lambdaRequest lambdaFunction "GET" "/definitions/someSlug3" None
              test <@ (getResponse.StatusCode, getResponse.Body) = (200, simpleDefinitions.Value) @>
          } ]
