module Contextive.LanguageServer.Tests.GlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open Contextive.LanguageServer.Tests.Helpers

let newTestLogger () =
    let loggedMessagesStore = ResizeArray<string>()
    let awaiter = ConditionAwaiter.create ()

    {| loggedMessages = loggedMessagesStore
       logger =
        { Glossary.Logger.info =
            fun msg ->
                loggedMessagesStore.Add(msg)
                ConditionAwaiter.received awaiter msg } |},
    awaiter


let noopMailboxProcessor () =
    MailboxProcessor.Start(fun _ -> async { return () })

let newCreateClossary () =
    { Glossary.FileScanner = fun _ -> []
      Glossary.Log = { info = fun _ -> () }
      Glossary.RegisterWatchedFiles = fun _ _ -> ()
      Glossary.SubGlossaryOps = { Create = fun _ -> noopMailboxProcessor () } }

[<Literal>]
let private EXPECTED_GLOSSARY_FILE_GLOB = "**/*.contextive.yml"

let waitForMessage awaiter msg =
    async { return! ConditionAwaiter.waitFor awaiter (fun m -> m = msg) }


[<Tests>]
let tests =
    testList
        "LanguageServer.SubGlossary Tests"
        [

          testList
              "When creating a glossary"
              [ testAsync "It should scan for files" {

                    let mockFileScanner glob =
                        if glob = EXPECTED_GLOSSARY_FILE_GLOB then
                            [ "path1"; "path2" ]
                        else
                            []

                    let testLogger, logAwaiter = newTestLogger ()

                    let _ =
                        Glossary.create
                            { newCreateClossary () with
                                FileScanner = mockFileScanner
                                Log = testLogger.logger }

                    let! res = waitForMessage logAwaiter "Found definitions file at 'path2'..."

                    test <@ testLogger.loggedMessages.Contains("Found definitions file at 'path1'...") @>
                    test <@ testLogger.loggedMessages.Contains("Found definitions file at 'path2'...") @>
                    test <@ testLogger.loggedMessages.Count = 2 @>
                }

                testAsync "It should create a glossary for each discovered file" {

                    let awaiter = ConditionAwaiter.create ()

                    let mockFileScanner glob =
                        if glob = EXPECTED_GLOSSARY_FILE_GLOB then
                            [ "path1" ]

                        else
                            []

                    let createSubGlossary path =
                        ConditionAwaiter.received awaiter path
                        noopMailboxProcessor ()

                    let _ =
                        Glossary.create
                            { newCreateClossary () with
                                FileScanner = mockFileScanner
                                SubGlossaryOps = { Create = createSubGlossary } }

                    do! waitForMessage awaiter "path1" |> Async.Ignore
                }

                testAsync "It should register filewatcher for globbed files" {

                    let awaiter = ConditionAwaiter.create ()
                    let mockRegisterWatchedFiles glob _ = ConditionAwaiter.received awaiter glob

                    let _ =
                        Glossary.create
                            { newCreateClossary () with
                                RegisterWatchedFiles = mockRegisterWatchedFiles }

                    do! waitForMessage awaiter EXPECTED_GLOSSARY_FILE_GLOB |> Async.Ignore
                }

                ]

          testList
              "When setting the default file location"
              [ testAsync "It should start watching a file at that location" {
                    let awaiter = ConditionAwaiter.create ()
                    let mockRegisterWatchedFiles glob _ = ConditionAwaiter.received awaiter glob

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                RegisterWatchedFiles = mockRegisterWatchedFiles }

                    Glossary.setDefaultGlossaryFile glossary "path"

                    do! waitForMessage awaiter "path" |> Async.Ignore
                } ]

          ]
