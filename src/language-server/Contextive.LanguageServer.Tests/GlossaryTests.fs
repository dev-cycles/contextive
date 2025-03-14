module Contextive.LanguageServer.Tests.GlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer

module CA = Contextive.LanguageServer.Tests.Helpers.ConditionAwaiter

let newTestLogger () =
    let loggedMessagesStore = ResizeArray<string>()
    let awaiter = CA.create ()

    {| loggedMessages = loggedMessagesStore
       logger =
        { Glossary.Logger.info =
            fun msg ->
                loggedMessagesStore.Add(msg)
                CA.received awaiter msg } |},
    awaiter


let noopMailboxProcessor () =
    MailboxProcessor.Start(fun _ -> async { return () })

let noop () = ()
let noop1 _ = ()

let newCreateClossary () =
    { Glossary.FileScanner = fun _ -> []
      Glossary.Log = { info = noop1 }
      Glossary.RegisterWatchedFiles = fun _ _ -> noop
      Glossary.SubGlossaryOps =
        { Start = fun _ -> noopMailboxProcessor ()
          Reload = noop1 } }

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

                    do! CA.expectMessage logAwaiter "Found definitions file at 'path1'..."
                    do! CA.expectMessage logAwaiter "Found definitions file at 'path2'..."
                    test <@ testLogger.loggedMessages.Count = 2 @>
                }

                testAsync "It should start a subglossary for each discovered file" {

                    let awaiter = CA.create ()

                    let mockFileScanner glob =
                        if glob = EXPECTED_GLOSSARY_FILE_GLOB then
                            [ "path1" ]

                        else
                            []

                    let mockStartSubGlossary path =
                        CA.received awaiter path
                        noopMailboxProcessor ()

                    let _ =
                        Glossary.create
                            { newCreateClossary () with
                                FileScanner = mockFileScanner
                                SubGlossaryOps =
                                    { Start = mockStartSubGlossary
                                      Reload = noop1 } }

                    do! CA.expectMessage awaiter "path1"
                }

                testAsync "It should register filewatcher for globbed files" {

                    let awaiter = CA.create ()

                    let mockRegisterWatchedFiles _ glob =
                        CA.received awaiter glob
                        noop

                    let _ =
                        Glossary.create
                            { newCreateClossary () with
                                RegisterWatchedFiles = mockRegisterWatchedFiles }

                    do! CA.expectMessage awaiter EXPECTED_GLOSSARY_FILE_GLOB
                }

                ]

          testList
              "When setting the default file location"
              [ testAsync "It should start watching a file at that location" {
                    let awaiter = CA.create ()

                    let mockRegisterWatchedFiles _ glob =
                        CA.received awaiter glob
                        noop

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                RegisterWatchedFiles = mockRegisterWatchedFiles }

                    Glossary.setDefaultGlossaryFile glossary "path"

                    do! CA.expectMessage awaiter "path"
                }

                testAsync "It should create a subglossary at the watched location" {
                    let awaiter = CA.create ()

                    let mockStartSubGlossary path =
                        CA.received awaiter path
                        noopMailboxProcessor ()

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                SubGlossaryOps =
                                    { Start = mockStartSubGlossary
                                      Reload = noop1 } }


                    Glossary.setDefaultGlossaryFile glossary "path1"

                    do! CA.expectMessage awaiter "path1"
                }

                testAsync "It should stop watching the old file if a new file is provided" {
                    let awaiter = CA.create ()

                    let mockRegisterWatchedFiles _ glob = (fun () -> CA.received awaiter true)

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                RegisterWatchedFiles = mockRegisterWatchedFiles }

                    Glossary.setDefaultGlossaryFile glossary "path1"

                    Glossary.setDefaultGlossaryFile glossary "path2"

                    do! CA.expectMessage awaiter true
                } ]

          testList
              "Given watched files"
              [ testList
                    "When watched file"
                    [ testAsync "is created it should start a subglossary for the watched file" {
                          let watchedFilesAwaiter = CA.create ()

                          let mockRegisterWatchedFiles watchedFilesHandlers glob =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let subGlossaryCreatedAwaiter = CA.create ()

                          let mockStartSubGlossary path =
                              CA.received subGlossaryCreatedAwaiter path
                              noopMailboxProcessor ()

                          let glossary =
                              Glossary.create
                                  { newCreateClossary () with
                                      RegisterWatchedFiles = mockRegisterWatchedFiles
                                      SubGlossaryOps =
                                          { Start = mockStartSubGlossary
                                            Reload = noop1 } }


                          let! watchedFilesHandlers = CA.waitForAny watchedFilesAwaiter

                          match watchedFilesHandlers with
                          | None -> test <@ watchedFilesHandlers.IsSome @>
                          | Some wfh -> wfh.OnCreated "pathA"

                          do! CA.expectMessage subGlossaryCreatedAwaiter "pathA"

                      }


                      testAsync "is updated it should reload the subglossary" {
                          let watchedFilesAwaiter = CA.create ()

                          let mockRegisterWatchedFiles watchedFilesHandlers glob =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let subGlossaryReloadedAwaiter = CA.create ()

                          let subGlossary = noopMailboxProcessor ()

                          let mockStartSubGlossary path = subGlossary

                          let mockReloadSubGlossary subGlossary =
                              CA.received subGlossaryReloadedAwaiter subGlossary
                              ()

                          let glossary =
                              Glossary.create
                                  { newCreateClossary () with
                                      RegisterWatchedFiles = mockRegisterWatchedFiles
                                      SubGlossaryOps =
                                          { Start = mockStartSubGlossary
                                            Reload = mockReloadSubGlossary } }


                          let! watchedFilesHandlers = CA.waitForAny watchedFilesAwaiter

                          match watchedFilesHandlers with
                          | None -> test <@ watchedFilesHandlers.IsSome @>
                          | Some wfh ->
                              wfh.OnCreated "pathA"
                              wfh.OnChanged "pathA"

                          do! CA.expectMessage subGlossaryReloadedAwaiter subGlossary

                      } ]




                testList
                    "When default file"
                    [ testAsync "is created it should reload the existing glossary for the default file" {
                          let watchedFilesAwaiter = CA.create ()

                          let mockRegisterWatchedFiles watchedFilesHandlers glob =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let subGlossaryReloadedAwaiter = CA.create ()

                          let subGlossary = noopMailboxProcessor ()

                          let defaultGlossaryCreatedAwaiter = CA.create ()

                          let mockStartSubGlossary path =
                              CA.received defaultGlossaryCreatedAwaiter path
                              subGlossary

                          let mockReloadSubGlossary subGlossary =
                              CA.received subGlossaryReloadedAwaiter subGlossary
                              ()

                          let glossary =
                              Glossary.create
                                  { newCreateClossary () with
                                      RegisterWatchedFiles = mockRegisterWatchedFiles
                                      SubGlossaryOps =
                                          { Start = mockStartSubGlossary
                                            Reload = mockReloadSubGlossary } }

                          Glossary.setDefaultGlossaryFile glossary "pathA"

                          do! CA.expectMessage defaultGlossaryCreatedAwaiter "pathA"

                          let! watchedFilesHandlers = CA.waitForAny watchedFilesAwaiter

                          match watchedFilesHandlers with
                          | None -> test <@ watchedFilesHandlers.IsSome @>
                          | Some wfh -> wfh.OnCreated "pathA"

                          do! CA.expectMessage subGlossaryReloadedAwaiter subGlossary

                      }


                      testAsync "is updated it should reload the subglossary" {
                          let watchedFilesAwaiter = CA.create ()

                          let mockRegisterWatchedFiles watchedFilesHandlers glob =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let subGlossaryReloadedAwaiter = CA.create ()

                          let subGlossary = noopMailboxProcessor ()

                          let defaultGlossaryCreatedAwaiter = CA.create ()

                          let mockStartSubGlossary path =
                              CA.received defaultGlossaryCreatedAwaiter path
                              subGlossary

                          let mockReloadSubGlossary subGlossary =
                              CA.received subGlossaryReloadedAwaiter subGlossary
                              ()

                          let glossary =
                              Glossary.create
                                  { newCreateClossary () with
                                      RegisterWatchedFiles = mockRegisterWatchedFiles
                                      SubGlossaryOps =
                                          { Start = mockStartSubGlossary
                                            Reload = mockReloadSubGlossary } }

                          Glossary.setDefaultGlossaryFile glossary "pathA"

                          do! CA.expectMessage defaultGlossaryCreatedAwaiter "pathA"

                          let! watchedFilesHandlers = CA.waitForAny watchedFilesAwaiter

                          match watchedFilesHandlers with
                          | None -> test <@ watchedFilesHandlers.IsSome @>
                          | Some wfh -> wfh.OnChanged "pathA"

                          do! CA.expectMessage subGlossaryReloadedAwaiter subGlossary

                      } ] ] ]
