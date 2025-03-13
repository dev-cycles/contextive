module Contextive.LanguageServer.Tests.GlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer

let newTestLogger () =
    let loggedMessagesStore = ResizeArray<string>()

    {| loggedMessages = loggedMessagesStore
       logger = { Glossary.Logger.info = fun msg -> loggedMessagesStore.Add(msg) } |}


let newCreateClossary () =
    { Glossary.FileScanner = fun _ -> []
      Glossary.Log = { info = fun _ -> () }
      Glossary.RegisterWatchedFiles = fun _ _ -> () }

[<Literal>]
let private EXPECTED_GLOSSARY_FILE_GLOB = "**/*.contextive.yml"

[<Tests>]
let tests =
    testList
        "LanguageServer.SubGlossary Tests"
        [

          testList
              "When creating a glossary"
              [ testAsync "It should scan for files" {

                    let fileScanner glob =
                        if glob = EXPECTED_GLOSSARY_FILE_GLOB then
                            [ "path1"; "path2" ]
                        else
                            []

                    let testLogger = newTestLogger ()

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                FileScanner = fileScanner
                                Log = testLogger.logger }


                    test <@ testLogger.loggedMessages.Contains("Found definitions file at 'path1'...") @>
                    test <@ testLogger.loggedMessages.Contains("Found definitions file at 'path2'...") @>
                    test <@ testLogger.loggedMessages.Count = 2 @>
                }

                testAsync "It should register filewatcher for globbed files" {

                    let mutable receivedWatchedFilesGlob = ""
                    let registerWatchedFiles glob _ = receivedWatchedFilesGlob <- glob

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                RegisterWatchedFiles = registerWatchedFiles }

                    test <@ receivedWatchedFilesGlob = EXPECTED_GLOSSARY_FILE_GLOB @>
                } ]

          ]
