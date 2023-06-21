module Contextive.LanguageServer.Tests.ConfigurationTests

open Expecto
open Swensen.Unquote
open System.IO
open Helpers.TestClient
open Contextive.LanguageServer.Tests.Helpers

[<Tests>]
let definitionsTests =
    testList
        "LanguageServer.Configuration Tests"
        [ testAsync "Can receive configuration value" {
              let config =
                  [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.contextivePathOptionsBuilder "one.yml" ]

              use! client = TestClient(config) |> init

              let! labels = Completion.getCompletionLabels client

              test <@ (labels, Fixtures.One.expectedCompletionLabels) ||> Seq.compareWith compare = 0 @>
          }

          testAsync "Can handle configuration value changing" {
              let mutable path = "one.yml"

              let pathLoader () : obj = path

              let config =
                  [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.contextivePathLoaderOptionsBuilder pathLoader ]

              use! client = TestClient(config) |> init

              path <- "two.yml"
              ConfigurationSection.didChange client path

              let! labels = Completion.getCompletionLabels client

              test <@ (labels, Fixtures.Two.expectedCompletionLabels) ||> Seq.compareWith compare = 0 @>

          }

          ]
