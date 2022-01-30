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

        let getFileName definitionsFileName = Path.Combine("fixtures", "completion_tests", $"{definitionsFileName}.yml") |> Some

        let getTermsFromFileInContext termFileUri definitionsFileName = async {
            let definitionsPath = getFileName definitionsFileName
            let workspaceFolder = Some ""
            let configGetter = (fun _ -> async.Return definitionsPath)
            let! definitions = getDefinitions configGetter workspaceFolder
            return! Definitions.find definitions termFileUri (fun _ -> true)
        }

        let getTermsFromFile = getTermsFromFileInContext ""

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

        testAsync "Can load multi-context definitions" {
            let! terms = getTermsFromFileInContext "/primary/secondary/test.txt" "multi" // Path contains both context's globs
            let foundNames = terms |> Seq.map (fun t -> t.Name)
            test <@ (foundNames, seq ["termInPrimary"; "termInSecondary"]) ||> compareList = 0 @>
        }

        let testInPath ((path:string),expectedTerms) =
            let pathName = path.Replace(".", "_dot_")
            testAsync $"with path {pathName}, expecting {expectedTerms}" {
                let! terms = getTermsFromFileInContext path "multi"
                let foundNames = terms |> Seq.map (fun t -> t.Name)
                test <@ (foundNames, seq expectedTerms) ||> compareList = 0 @>
            }
        [
            ("/some/path/with/primary/in/it.txt", ["termInPrimary"])
            ("/some/path/with/primary.txt", ["termInPrimary"])
            ("/some/path/with/primary", ["termInPrimary"])
            ("/some/path/with/test.js", ["termInPrimary"])
            ("/primary", ["termInPrimary"])
            ("/some/path/with/secondary/in/it.txt", ["termInSecondary"])
            ("/some/path/with/secondary.txt", ["termInSecondary"])
            ("/some/path/with/secondary", ["termInSecondary"])
            ("/secondary", ["termInSecondary"])
            ("/some/path", [])
            ("/some/path/test.cs", [])
        ] |> List.map testInPath |> testList "Can load definition from correct context"

        let invalidScenarios =
            [
                ("invalid_empty","Error loading definitions: Definitions file is empty.")
                ("invalid_schema","Error loading definitions: Error parsing definitions file:  Object starting line 4, column 7 - Property 'example' not found on type 'Contextive.LanguageServer.Definitions+Term'.")
                ("invalid_schema2","Error loading definitions: Error parsing definitions file:  Object starting line 5, column 19 - (Line: 5, Col: 19, Idx: 131) - (Line: 5, Col: 19, Idx: 131): Mapping values are not allowed in this context.")
            ]

        let canRecoverFromInvalidDefinitions (fileName, expectedErrorMessage) =
            testAsync fileName {
                let mutable path = getFileName fileName

                let workspaceFolder = Some ""
                let configGetter = (fun _ -> async.Return path)

                let errorMessageAwaiter = ConditionAwaiter.create()

                let onErrorLoading = fun msg -> ConditionAwaiter.received errorMessageAwaiter msg

                let! definitions = getDefinitionsWithErrorHandler onErrorLoading configGetter workspaceFolder
                let! termsWhenInvalid = Definitions.find definitions "" (fun _ -> true)

                let! errorMessage = ConditionAwaiter.waitForAny errorMessageAwaiter 500
                test <@ errorMessage.Value = expectedErrorMessage @>
                test <@ Seq.length termsWhenInvalid = 0 @>

                path <- getFileName "one"
                (Definitions.loader definitions)()

                let! termsWhenValid = Definitions.find definitions "" (fun _ -> true)
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