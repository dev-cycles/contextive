module Contextive.LanguageServer.Tests.DefinitionsTests

open Expecto
open Swensen.Unquote
open System.IO
open Contextive.LanguageServer
open TestClient

[<Tests>]
let definitionsTests =
    testList "Definitions File Tests" [

        let getDefinitionsWithErrorHandler onErrorAction configGetter workspaceFolder  = async {
            let definitions = Definitions.create()
            Definitions.init definitions (fun _ -> ()) configGetter None onErrorAction
            Definitions.addFolder definitions workspaceFolder
            (Definitions.loader definitions)()
            return definitions
        }

        let getDefinitions = getDefinitionsWithErrorHandler (fun _ -> ())

        let getFileName fileName = Path.Combine("fixtures", "completion_tests", $"{fileName}.yml") |> Some

        let getTermsFromFile fileName = async {
            let path = getFileName fileName
            let workspaceFolder = Some ""
            let configGetter = (fun _ -> async.Return path)
            let! definitions = getDefinitions configGetter workspaceFolder
            return! Definitions.find definitions (fun _ -> true)
        }

        let compareList = Seq.compareWith compare

        let oneExpectedNames = seq ["firstTerm"; "secondTerm"; "thirdTerm"]

        testAsync "Can load term Names" {
            let! terms = getTermsFromFile "one"
            let foundNames = terms |> Seq.map (fun t -> t.Name)
            test <@ (foundNames, oneExpectedNames) ||> compareList = 0 @>
        }

        testAsync "Can load term Definitions" {
            let! terms = getTermsFromFile "one"
            let foundDefinitions = terms |> Seq.map (fun t -> t.Definition) |> Seq.choose id
            let expectedDefinitions = seq ["The first term in our definitions list"; "The second term in our definitions list"]
            test <@ (foundDefinitions, expectedDefinitions) ||> compareList = 0 @>
        }

        testAsync "Can load term UsageExamples" {
            let! terms = getTermsFromFile "one"
            let foundDefinitions = terms |> Seq.map (fun t -> t.Examples) |> Seq.filter ((<>) null) |> Seq.map Seq.cast
            let expectedDefinitions = seq [seq ["An arbitrary usage of secondTerm";"Don't forget to secondTerm the firstTerms"]]
            test <@ (foundDefinitions, expectedDefinitions) ||> Seq.compareWith compareList = 0 @>
        }

        let invalidScenarios =
            [
                ("invalid_empty","Error loading definitions: Definitions file is empty.")
                ("invalid_schema","Error loading definitions: Error parsing definitions file:  Object starting line 4, column 7 - Property 'example' not found on type 'Contextive.LanguageServer.Definitions+Term'.")
            ]

        let canRecoverFromInvalidDefinitions (fileName, expectedErrorMessage) =
            testAsync fileName {
                let mutable path = getFileName fileName

                let workspaceFolder = Some ""
                let configGetter = (fun _ -> async.Return path)

                let errorMessageAwaiter = ConditionAwaiter.create()

                let onErrorLoading = fun msg -> ConditionAwaiter.received errorMessageAwaiter msg

                let! definitions = getDefinitionsWithErrorHandler onErrorLoading configGetter workspaceFolder
                let! termsWhenInvalid = Definitions.find definitions (fun _ -> true)

                let! errorMessage = ConditionAwaiter.waitForAny errorMessageAwaiter 500
                test <@ errorMessage.Value = expectedErrorMessage @>
                test <@ Seq.length termsWhenInvalid = 0 @>

                path <- getFileName "one"
                (Definitions.loader definitions)()

                let! termsWhenValid = Definitions.find definitions (fun _ -> true)
                let foundNames = termsWhenValid |> Seq.map (fun t -> t.Name)

                test <@ (foundNames, oneExpectedNames) ||> compareList = 0 @>
            }
        
        invalidScenarios |> List.map canRecoverFromInvalidDefinitions |> testList "Can recover from invalid definitions"

        let canRecoverFromInvalidDefinitionsInNewConfig (fileName, _) =
            testAsync fileName {
                let validPath = "one.yml"
                let mutable path = validPath

                let pathLoader():obj = path

                let config = [
                    Workspace.optionsBuilder <| Path.Combine("fixtures", "completion_tests")
                    ConfigurationSection.contextivePathLoaderOptionsBuilder pathLoader
                ]

                use! client = TestClient(config) |> init

                let! termsWhenValidAtStart = Completion.getCompletionLabels client
                test <@ (termsWhenValidAtStart, oneExpectedNames) ||> compareList = 0 @>

                path <- $"{fileName}.yml"
                ConfigurationSection.didChange client path

                let! termsWhenInvalid = Completion.getCompletionLabels client
                test <@ Seq.length termsWhenInvalid = 0 @>

                path <- validPath
                ConfigurationSection.didChange client path

                let! termsWhenValidAtEnd = Completion.getCompletionLabels client
                test <@ (termsWhenValidAtEnd, oneExpectedNames) ||> compareList = 0 @>
            }
        
        invalidScenarios |> List.map canRecoverFromInvalidDefinitionsInNewConfig |> testList "Can recover from invalid definitions in fresh config"
    ]