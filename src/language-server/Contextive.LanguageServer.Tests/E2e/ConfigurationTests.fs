module Contextive.LanguageServer.Tests.E2e.ConfigurationTests

open Expecto
open Swensen.Unquote
open System.IO
open Contextive.LanguageServer.Tests.Helpers.TestClient
open Contextive.LanguageServer.Tests.Helpers

[<Tests>]
let tests =
    testList
        "LanguageServer.Configuration Tests"
        [ testAsync "Can receive configuration value" {
              let config =
                  [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.contextivePathBuilder "one.yml" ]

              use! client = TestClient(config) |> init

              let! labels = Completion.getCompletionLabels client

              test <@ (labels, Fixtures.One.expectedCompletionLabels) ||> Seq.compareWith compare = 0 @>
          }

          testAsync "Can handle configuration value changing" {
              let mutable path = "one.yml"

              let basePath =
                  Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "completion_tests")

              let pathLoader () : obj = path

              let config =
                  [ Workspace.optionsBuilder <| basePath
                    ConfigurationSection.contextivePathLoaderBuilder pathLoader ]

              let! client, logAwaiter = TestClient(config) |> initAndGetLogAwaiter
              use client = client

              path <- "two.yml"
              do! ConfigurationSection.didChangePath client path logAwaiter

              let! labels = Completion.getCompletionLabelsInPath basePath client

              test <@ (labels, Fixtures.Two.expectedCompletionLabels) ||> Seq.compareWith compare = 0 @>

          }

          testAsync "Can handle client that doesn't support didChangeConfiguration" {
              let mutable path = "one.yml"

              let config =
                  [ Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.contextivePathBuilder path ]

              use! client = TestClient(config) |> init

              let! labels = Completion.getCompletionLabels client

              test <@ (labels, Fixtures.One.expectedCompletionLabels) ||> Seq.compareWith compare = 0 @>

          }

          ]
