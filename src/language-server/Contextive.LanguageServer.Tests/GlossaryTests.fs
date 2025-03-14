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

let noop () = ()

let newCreateClossary () =
    { Glossary.FileScanner = fun _ -> []
      Glossary.Log = { info = fun _ -> () }
      Glossary.RegisterWatchedFiles = fun _ _ -> noop
      Glossary.SubGlossaryOps = { Create = fun _ -> noopMailboxProcessor () } }

[<Literal>]
let private EXPECTED_GLOSSARY_FILE_GLOB = "**/*.contextive.yml"

let waitForMessage awaiter msg =
    async { return! ConditionAwaiter.waitFor awaiter (fun m -> m = msg) }

let expectMessage awaiter msg =
    async {
        let! m = waitForMessage awaiter msg
        test <@ m = Some msg @>
    }

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

                    do! expectMessage logAwaiter "Found definitions file at 'path1'..."
                    do! expectMessage logAwaiter "Found definitions file at 'path2'..."
                    test <@ testLogger.loggedMessages.Count = 2 @>
                }

                testAsync "It should create a glossary for each discovered file" {

                    let awaiter = ConditionAwaiter.create ()

                    let mockFileScanner glob =
                        if glob = EXPECTED_GLOSSARY_FILE_GLOB then
                            [ "path1" ]

                        else
                            []

                    let mockCreateSubGlossary path =
                        ConditionAwaiter.received awaiter path
                        noopMailboxProcessor ()

                    let _ =
                        Glossary.create
                            { newCreateClossary () with
                                FileScanner = mockFileScanner
                                SubGlossaryOps = { Create = mockCreateSubGlossary } }

                    do! expectMessage awaiter "path1"
                }

                testAsync "It should register filewatcher for globbed files" {

                    let awaiter = ConditionAwaiter.create ()

                    let mockRegisterWatchedFiles glob _ =
                        ConditionAwaiter.received awaiter glob
                        noop

                    let _ =
                        Glossary.create
                            { newCreateClossary () with
                                RegisterWatchedFiles = mockRegisterWatchedFiles }

                    do! expectMessage awaiter EXPECTED_GLOSSARY_FILE_GLOB
                }

                ]

          testList
              "When setting the default file location"
              [ testAsync "It should start watching a file at that location" {
                    let awaiter = ConditionAwaiter.create ()

                    let mockRegisterWatchedFiles glob _ =
                        ConditionAwaiter.received awaiter glob
                        noop

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                RegisterWatchedFiles = mockRegisterWatchedFiles }

                    Glossary.setDefaultGlossaryFile glossary "path"

                    do! expectMessage awaiter "path"
                }

                testAsync "It should create a glossary at the watched location" {
                    let awaiter = ConditionAwaiter.create ()

                    let mockCreateSubGlossary path =
                        ConditionAwaiter.received awaiter path
                        noopMailboxProcessor ()

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                SubGlossaryOps = { Create = mockCreateSubGlossary } }


                    Glossary.setDefaultGlossaryFile glossary "path1"

                    do! expectMessage awaiter "path1"
                }

                testAsync "It should stop watching the old file if a new file is provided" {
                    let awaiter = ConditionAwaiter.create ()

                    let mockRegisterWatchedFiles glob _ =
                        (fun () -> ConditionAwaiter.received awaiter true)

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                RegisterWatchedFiles = mockRegisterWatchedFiles }

                    Glossary.setDefaultGlossaryFile glossary "path1"

                    Glossary.setDefaultGlossaryFile glossary "path2"

                    do! expectMessage awaiter true
                } ]

          ]
