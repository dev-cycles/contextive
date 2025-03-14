module Contextive.LanguageServer.Tests.GlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open System.Linq
open Helpers.SubGlossaryHelper

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
      Glossary.SubGlossaryOps =
        { Start = fun _ -> noopMailboxProcessor ()
          Reload = noop1 } }

let newInitGlossary () =
    { Glossary.Log = { info = noop1 }
      Glossary.DefaultSubGlossaryPathResolver = fun _ -> async.Return None
      Glossary.RegisterWatchedFiles = fun _ _ -> noop }

[<Literal>]
let private EXPECTED_GLOSSARY_FILE_GLOB = "**/*.contextive.yml"

[<Tests>]
let tests =

    let getName (t: Contextive.Core.GlossaryFile.Term) = t.Name
    let compareList = Seq.compareWith compare

    testList
        "LanguageServer.Glossary Tests"
        [

          testList
              "When creating a glossary"
              [ testAsync "It should start a subglossary for each discovered file" {

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

                    let glossary = newCreateClossary () |> Glossary.create

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            RegisterWatchedFiles = mockRegisterWatchedFiles }

                    do! CA.expectMessage awaiter <| Some EXPECTED_GLOSSARY_FILE_GLOB
                }

                ]

          testList
              "When setting the default file location"
              [ testAsync "It should start watching a file at that location" {
                    let awaiter = CA.create ()

                    let mockRegisterWatchedFiles _ glob =
                        CA.received awaiter glob
                        noop

                    let glossary = newCreateClossary () |> Glossary.create

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            RegisterWatchedFiles = mockRegisterWatchedFiles
                            DefaultSubGlossaryPathResolver = fun () -> async.Return <| Some "path" }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage awaiter <| Some "path"
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

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            DefaultSubGlossaryPathResolver = fun () -> async.Return <| Some "path1" }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage awaiter "path1"
                }

                testAsync "It should stop watching the old file if a new file is provided" {
                    let awaiter = CA.create ()

                    let mockRegisterWatchedFiles _ glob = (fun () -> CA.received awaiter true)

                    let glossary = newCreateClossary () |> Glossary.create

                    let mutable currentPath = "path1"

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            DefaultSubGlossaryPathResolver = fun () -> currentPath |> Some |> async.Return
                            RegisterWatchedFiles = mockRegisterWatchedFiles }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    currentPath <- "path2"

                    Glossary.reloadDefaultGlossaryFile glossary ()

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
                                      SubGlossaryOps =
                                          { Start = mockStartSubGlossary
                                            Reload = noop1 } }

                          Glossary.init
                              glossary
                              { newInitGlossary () with
                                  RegisterWatchedFiles = mockRegisterWatchedFiles }


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
                                      SubGlossaryOps =
                                          { Start = mockStartSubGlossary
                                            Reload = mockReloadSubGlossary } }

                          Glossary.init
                              glossary
                              { newInitGlossary () with
                                  RegisterWatchedFiles = mockRegisterWatchedFiles }


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
                                      SubGlossaryOps =
                                          { Start = mockStartSubGlossary
                                            Reload = mockReloadSubGlossary } }

                          Glossary.init
                              glossary
                              { newInitGlossary () with
                                  DefaultSubGlossaryPathResolver = fun () -> "pathA" |> Some |> async.Return
                                  RegisterWatchedFiles = mockRegisterWatchedFiles }

                          Glossary.reloadDefaultGlossaryFile glossary ()

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
                                      SubGlossaryOps =
                                          { Start = mockStartSubGlossary
                                            Reload = mockReloadSubGlossary } }

                          Glossary.init
                              glossary
                              { newInitGlossary () with
                                  DefaultSubGlossaryPathResolver = fun () -> "pathA" |> Some|> async.Return
                                  RegisterWatchedFiles = mockRegisterWatchedFiles }

                          Glossary.reloadDefaultGlossaryFile glossary ()

                          do! CA.expectMessage defaultGlossaryCreatedAwaiter "pathA"

                          let! watchedFilesHandlers = CA.waitForAny watchedFilesAwaiter

                          match watchedFilesHandlers with
                          | None -> test <@ watchedFilesHandlers.IsSome @>
                          | Some wfh -> wfh.OnChanged "pathA"

                          do! CA.expectMessage subGlossaryReloadedAwaiter subGlossary

                      } ] ]
          testList
              "When doing a lookup"
              [ testAsync "with default subglossary only" {
                    let awaiter = CA.create ()

                    let fileReader p =
                        CA.received awaiter p

                        """contexts:
  - terms:
    - name: subGlossary1"""
                        |> Ok


                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                SubGlossaryOps =
                                    { Start = NSubGlossary.start fileReader
                                      Reload = NSubGlossary.reload } }

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            DefaultSubGlossaryPathResolver = fun () -> Helpers.Fixtures.One.path |> Some |> async.Return}

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    let! result = Glossary.lookup glossary "" id

                    test <@ result.Count() = 1 @>

                } ]
          testList
              "Integration"
              [ testAsync "Can collaborate with subGlossaries" {
                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                SubGlossaryOps =
                                    { Start = NSubGlossary.start FileReader.pathReader
                                      Reload = NSubGlossary.reload } }

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            DefaultSubGlossaryPathResolver = fun () -> Helpers.Fixtures.One.path |> Some |> async.Return}

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    let! result = Glossary.lookup glossary "" id

                    test <@ result.Count() = 1 @>

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ (foundNames, Helpers.Fixtures.One.expectedTerms) ||> compareList = 0 @>
                } ] ]
