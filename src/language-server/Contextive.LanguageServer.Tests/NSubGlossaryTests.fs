module Contextive.LanguageServer.Tests.NSubGlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open System.Linq
open Helpers.SubGlossaryHelper

module CA = Contextive.LanguageServer.Tests.Helpers.ConditionAwaiter

[<Tests>]
let tests =

    let emptyGlossary =
        """contexts:
  - terms:"""

    testList
        "LanguageServer.SubGlossary Tests"
        [ testAsync "When starting a subglossary it should read the file at the provided path" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p
                  emptyGlossary

              let _ = NSubGlossary.create fileReader "path1"

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When reloading a subglossary it should read the file at the path provided when it was created" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p
                  emptyGlossary

              let subGlossary = NSubGlossary.create fileReader "path1"

              do! CA.expectMessage awaiter "path1"

              CA.clear awaiter

              NSubGlossary.reload subGlossary

              do! CA.expectMessage awaiter "path1"
          }

          testAsync "When looking up a term in a subglossary it should return the matching terms" {
              let awaiter = CA.create ()

              let fileReader p =
                  CA.received awaiter p

                  """contexts:
  - terms:
    - name: subGlossary1"""

              let subGlossary = NSubGlossary.create fileReader "path1"

              let! result = NSubGlossary.lookup subGlossary id

              let terms = FindResult.allTerms result

              test <@ result.Count() = 1 @>
              test <@ (terms |> Seq.head).Name = "subGlossary1" @>
          }

          ]
