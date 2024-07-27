module Contextive.Cloud.Tests.E2e.DefinitionsSyncTests

open Expecto
open Swensen.Unquote
open FSharp.Data
open HttpRequestHeaders
open Contextive.Cloud.Tests.E2e

let baseUrl = ApiHelper.baseUrl
let testSlug = "Tests.E2e"

[<Tests>]
let definitionsSyncTests =
    testList
        "Definitions Sync"
        [

          testCase "Can push definitions file into Contextive Cloud"
          <| fun () ->
              let res =
                  Http.Request(
                      $"{baseUrl}/definitions/{testSlug}",
                      headers = [ ContentType "text/yml" ],
                      httpMethod = HttpMethod.Put,
                      silentHttpErrors = true,
                      body =
                          TextRequest
                              """
contexts:
    - terms:
        - name: testTerm"""
                  )

              printfn "%A" res
              test <@ res.StatusCode = HttpStatusCodes.OK @>

          ]
