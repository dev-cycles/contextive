module Contextive.LanguageServer.Tests.Component.GlossaryManagerTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open System.Linq
open Tests.Helpers.GlossaryHelper
open Contextive.Core.File
open Contextive.LanguageServer.Tests.Helpers

module CA = ConditionAwaiter

let newTestLogger () =
    let loggedMessagesStore = ResizeArray<string>()
    let awaiter = CA.create ()

    {| loggedMessages = loggedMessagesStore
       logger =
        { Logger.Logger.Noop with
            info =
                fun msg ->
                    loggedMessagesStore.Add(msg)
                    CA.received awaiter msg } |},
    awaiter


let noopMailboxProcessor () =
    MailboxProcessor.Start(fun _ -> async { return () })

let noop () = ()
let noop1 _ = ()

let newCreateGlossary () =
    { GlossaryManager.GlossaryOps =
        { Start = fun _ -> noopMailboxProcessor ()
          Reload = noop1
          Stop = noop1 } }

let newInitGlossary () =
    { GlossaryManager.FileScanner = fun _ -> []
      GlossaryManager.Log = Logger.Logger.Noop
      GlossaryManager.DefaultGlossaryPathResolver = fun _ -> async.Return <| Error FileError.NotYetLoaded
      GlossaryManager.RegisterWatchedFiles = fun _ _ -> noop }

let private EXPECTED_GLOSSARY_FILE_GLOB =
    [| "**/*.glossary.yml"; "**/*.glossary.yaml" |]

[<Tests>]
let tests =

    let getName (t: Contextive.Core.GlossaryFile.Term) = t.Name
    let compareList = Seq.compareWith compare

    let pc p : PathConfiguration = { Path = p; Source = Configured }

    testList
        "LanguageServer.GlossaryManager Tests"
        [

          testList
              "When creating a glossary manager"
              [ testAsync "It should start a glossary for each discovered file" {

                    let awaiter = CA.create ()

                    let mockFileScanner glob =
                        if glob = EXPECTED_GLOSSARY_FILE_GLOB then
                            seq { "path1" }
                        else
                            Seq.empty

                    let mockStartGlossary (path: Glossary.StartGlossary) =
                        CA.received awaiter path.Path.Path
                        noopMailboxProcessor ()

                    let glossary =
                        GlossaryManager.create
                            { newCreateGlossary () with
                                GlossaryOps =
                                    { Start = mockStartGlossary
                                      Reload = noop1
                                      Stop = noop1 } }

                    GlossaryManager.init glossary
                    <| { newInitGlossary () with
                           FileScanner = mockFileScanner }

                    do! CA.expectMessage awaiter "path1"
                }

                testAsync "It should register filewatcher for globbed files" {

                    let awaiter = CA.create ()

                    let mockRegisterWatchedFiles _ glob =
                        CA.received awaiter glob
                        noop

                    let glossary = newCreateGlossary () |> GlossaryManager.create

                    GlossaryManager.init
                        glossary
                        { newInitGlossary () with
                            RegisterWatchedFiles = mockRegisterWatchedFiles }

                    do! CA.expectMessage awaiter <| EXPECTED_GLOSSARY_FILE_GLOB
                }

                ]

          testList
              "When setting the default file location"
              [ testAsync "It should start watching a file at that location" {
                    let awaiter = CA.create ()

                    let mockRegisterWatchedFiles _ glob =
                        CA.received awaiter glob
                        noop

                    let glossary = newCreateGlossary () |> GlossaryManager.create

                    GlossaryManager.init
                        glossary
                        { newInitGlossary () with
                            RegisterWatchedFiles = mockRegisterWatchedFiles
                            DefaultGlossaryPathResolver = fun () -> "path" |> pc |> Ok |> async.Return }

                    GlossaryManager.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage awaiter <| [| "path" |]
                }

                testAsync "It should create a glossary at the watched location" {
                    let awaiter = CA.create ()

                    let mockStartGlossary (path: Glossary.StartGlossary) =
                        CA.received awaiter path.Path.Path
                        noopMailboxProcessor ()

                    let glossary =
                        GlossaryManager.create
                            { newCreateGlossary () with
                                GlossaryOps =
                                    { Start = mockStartGlossary
                                      Reload = noop1
                                      Stop = noop1 } }

                    GlossaryManager.init
                        glossary
                        { newInitGlossary () with
                            DefaultGlossaryPathResolver = fun () -> "path1" |> pc |> Ok |> async.Return }

                    GlossaryManager.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage awaiter "path1"
                }

                testAsync "It should stop watching the old file if a new file is provided" {
                    let awaiter = CA.create ()

                    let mockRegisterWatchedFiles _ _ = (fun () -> CA.received awaiter true)

                    let glossary = newCreateGlossary () |> GlossaryManager.create

                    let mutable currentPath = "path1"

                    GlossaryManager.init
                        glossary
                        { newInitGlossary () with
                            DefaultGlossaryPathResolver = fun () -> currentPath |> pc |> Ok |> async.Return
                            RegisterWatchedFiles = mockRegisterWatchedFiles }

                    GlossaryManager.reloadDefaultGlossaryFile glossary ()

                    currentPath <- "path2"

                    GlossaryManager.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage awaiter true
                }

                testAsync "It should log and error if unable to get the path of the configured filed" {
                    let errorAwaiter = CA.create ()

                    let glossary = newCreateGlossary () |> GlossaryManager.create

                    GlossaryManager.init
                        glossary
                        { newInitGlossary () with
                            Log =
                                { Logger.Logger.Noop with
                                    error = CA.received errorAwaiter }
                            DefaultGlossaryPathResolver =
                                fun () -> Error(FileError.PathInvalid "testing error.") |> async.Return }

                    GlossaryManager.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage errorAwaiter "Error loading glossary file: Invalid Path: testing error."
                }

                ]

          testList
              "Given watched files"
              [ testList
                    "When watched file"
                    [ testAsync "is created it should start a glossary for the watched file" {
                          let watchedFilesAwaiter = CA.create ()

                          let mockRegisterWatchedFiles watchedFilesHandlers _ =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let glossaryCreatedAwaiter = CA.create ()

                          let mockStartGlossary (path: Glossary.StartGlossary) =
                              CA.received glossaryCreatedAwaiter path.Path.Path
                              noopMailboxProcessor ()

                          let glossary =
                              GlossaryManager.create
                                  { newCreateGlossary () with
                                      GlossaryOps =
                                          { Start = mockStartGlossary
                                            Reload = noop1
                                            Stop = noop1 } }

                          GlossaryManager.init
                              glossary
                              { newInitGlossary () with
                                  RegisterWatchedFiles = mockRegisterWatchedFiles }


                          let! watchedFilesHandlers = CA.waitForAny watchedFilesAwaiter

                          match watchedFilesHandlers with
                          | None -> test <@ watchedFilesHandlers.IsSome @>
                          | Some wfh -> wfh.OnCreated "pathA"

                          do! CA.expectMessage glossaryCreatedAwaiter "pathA"

                      }


                      testAsync "is updated it should reload the glossary" {
                          let watchedFilesAwaiter = CA.create ()

                          let mockRegisterWatchedFiles watchedFilesHandlers _ =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let glossaryReloadedAwaiter = CA.create ()

                          let glossary = noopMailboxProcessor ()

                          let mockStartGlossary _ = glossary

                          let mockReloadGlossary glossary =
                              CA.received glossaryReloadedAwaiter glossary
                              ()

                          let glossaryManager =
                              GlossaryManager.create
                                  { newCreateGlossary () with
                                      GlossaryOps =
                                          { Start = mockStartGlossary
                                            Reload = mockReloadGlossary
                                            Stop = noop1 } }

                          GlossaryManager.init
                              glossaryManager
                              { newInitGlossary () with
                                  RegisterWatchedFiles = mockRegisterWatchedFiles }


                          let! watchedFilesHandlers = CA.waitForAny watchedFilesAwaiter

                          match watchedFilesHandlers with
                          | None -> test <@ watchedFilesHandlers.IsSome @>
                          | Some wfh ->
                              wfh.OnCreated "pathA"
                              wfh.OnChanged "pathA"

                          do! CA.expectMessage glossaryReloadedAwaiter glossary

                      } ]




                testList
                    "When default file"
                    [ testAsync "is created it should reload the existing glossary for the default file" {
                          let watchedFilesAwaiter = CA.create ()

                          let mockRegisterWatchedFiles watchedFilesHandlers _ =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let glossaryReloadedAwaiter = CA.create ()

                          let glossary = noopMailboxProcessor ()

                          let defaultGlossaryCreatedAwaiter = CA.create ()

                          let mockStartGlossary (path: Glossary.StartGlossary) =
                              CA.received defaultGlossaryCreatedAwaiter path.Path.Path
                              glossary

                          let mockReloadGlossary glossary =
                              CA.received glossaryReloadedAwaiter glossary
                              ()

                          let glossaryManager =
                              GlossaryManager.create
                                  { newCreateGlossary () with
                                      GlossaryOps =
                                          { Start = mockStartGlossary
                                            Reload = mockReloadGlossary
                                            Stop = noop1 } }

                          GlossaryManager.init
                              glossaryManager
                              { newInitGlossary () with
                                  DefaultGlossaryPathResolver = fun () -> "pathA" |> pc |> Ok |> async.Return
                                  RegisterWatchedFiles = mockRegisterWatchedFiles }

                          GlossaryManager.reloadDefaultGlossaryFile glossaryManager ()

                          do! CA.expectMessage defaultGlossaryCreatedAwaiter "pathA"

                          let! watchedFilesHandlers = CA.waitForAny watchedFilesAwaiter

                          match watchedFilesHandlers with
                          | None -> test <@ watchedFilesHandlers.IsSome @>
                          | Some wfh -> wfh.OnCreated "pathA"

                          do! CA.expectMessage glossaryReloadedAwaiter glossary

                      }


                      testAsync "is updated it should reload the glossary" {
                          let watchedFilesAwaiter = CA.create ()

                          let mockRegisterWatchedFiles watchedFilesHandlers _ =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let glossaryReloadedAwaiter = CA.create ()

                          let glossary = noopMailboxProcessor ()

                          let defaultGlossaryCreatedAwaiter = CA.create ()

                          let mockStartGlossary (path: Glossary.StartGlossary) =
                              CA.received defaultGlossaryCreatedAwaiter path.Path.Path
                              glossary

                          let mockReloadGlossary glossary =
                              CA.received glossaryReloadedAwaiter glossary
                              ()

                          let glossaryManager =
                              GlossaryManager.create
                                  { newCreateGlossary () with
                                      GlossaryOps =
                                          { Start = mockStartGlossary
                                            Reload = mockReloadGlossary
                                            Stop = noop1 } }

                          GlossaryManager.init
                              glossaryManager
                              { newInitGlossary () with
                                  DefaultGlossaryPathResolver = fun () -> "pathA" |> pc |> Ok |> async.Return
                                  RegisterWatchedFiles = mockRegisterWatchedFiles }

                          GlossaryManager.reloadDefaultGlossaryFile glossaryManager ()

                          do! CA.expectMessage defaultGlossaryCreatedAwaiter "pathA"

                          let! watchedFilesHandlers = CA.waitForAny watchedFilesAwaiter

                          match watchedFilesHandlers with
                          | None -> test <@ watchedFilesHandlers.IsSome @>
                          | Some wfh -> wfh.OnChanged "pathA"

                          do! CA.expectMessage glossaryReloadedAwaiter glossary

                      } ] ]
          testList
              "When doing a lookup"
              [ testAsync "when current file undefined" {
                    let awaiter = CA.create ()

                    let fileReader p =
                        CA.received awaiter p.Path

                        """contexts:
  - terms:
    - name: glossary1"""
                        |> Ok


                    let glossary =
                        GlossaryManager.create
                            { newCreateGlossary () with
                                GlossaryOps =
                                    { Start = Glossary.start fileReader
                                      Reload = Glossary.reload
                                      Stop = Glossary.stop } }

                    GlossaryManager.init
                        glossary
                        { newInitGlossary () with
                            DefaultGlossaryPathResolver = fun () -> Fixtures.One.path |> pc |> Ok |> async.Return }

                    GlossaryManager.reloadDefaultGlossaryFile glossary ()

                    let! result = GlossaryManager.lookup glossary "" id

                    test <@ result.Count() = 1 @>

                }

                testAsync "with default glossary only" {
                    let awaiter = CA.create ()

                    let fileReader p =
                        CA.received awaiter p.Path

                        """contexts:
  - terms:
    - name: glossary1"""
                        |> Ok


                    let glossary =
                        GlossaryManager.create
                            { newCreateGlossary () with
                                GlossaryOps =
                                    { Start = Glossary.start fileReader
                                      Reload = Glossary.reload
                                      Stop = Glossary.stop } }

                    GlossaryManager.init
                        glossary
                        { newInitGlossary () with
                            DefaultGlossaryPathResolver = fun () -> Fixtures.One.path |> pc |> Ok |> async.Return }

                    GlossaryManager.reloadDefaultGlossaryFile glossary ()

                    let! result = GlossaryManager.lookup glossary "/some/file/anywhere" id

                    test <@ result.Count() = 1 @>

                }

                testAsync "with default and one extra glossary" {
                    let filesMap =
                        Map
                            [ "/default/glossary.yml",
                              """contexts:
 - terms:
   - name: defaultGlossary"""
                              "/path2/path2.contextive.yml",
                              """contexts:
 - terms:
   - name: glossary2""" ]

                    let mockFileReader p = filesMap[p.Path] |> Ok

                    let mockFileScanner _ = seq { "/path2/path2.contextive.yml" }

                    let glossary =
                        GlossaryManager.create
                            { newCreateGlossary () with
                                GlossaryOps =
                                    { Start = Glossary.start mockFileReader
                                      Reload = Glossary.reload
                                      Stop = Glossary.stop } }

                    GlossaryManager.init
                        glossary
                        { newInitGlossary () with
                            FileScanner = mockFileScanner
                            DefaultGlossaryPathResolver = fun () -> "/default/glossary.yml" |> pc |> Ok |> async.Return }

                    GlossaryManager.reloadDefaultGlossaryFile glossary ()

                    let getNames (result: Contextive.Core.GlossaryFile.FindResult) =
                        result |> Seq.collect _.Terms |> Seq.map _.Name |> Set.ofSeq

                    let! result = GlossaryManager.lookup glossary "/path1/file.yml" id

                    test <@ result.Count() = 1 @>
                    test <@ result |> getNames |> Set.minElement = "defaultGlossary" @>

                    let! result = GlossaryManager.lookup glossary "/default/pathinsamefolderasdefault.txt" id
                    test <@ result.Count() = 1 @>
                    test <@ result |> getNames |> Set.minElement = "defaultGlossary" @>

                    let! result = GlossaryManager.lookup glossary "/path2/file.yml" id

                    test <@ result.Count() = 2 @>

                    let result2Terms = result |> getNames

                    let expectedTerms =
                        Set.ofSeq
                        <| seq {
                            "defaultGlossary"
                            "glossary2"
                        }

                    test <@ result2Terms = expectedTerms @>

                } ]
          testList
              "Integration"
              [ testAsync "Can collaborate with subGlossaries" {
                    let glossary =
                        GlossaryManager.create
                            { newCreateGlossary () with
                                GlossaryOps =
                                    { Start = Glossary.start FileReader.configuredReader
                                      Reload = Glossary.reload
                                      Stop = Glossary.stop } }

                    let startupAwaiter = CA.create ()

                    let path = Fixtures.One.path

                    GlossaryManager.init
                        glossary
                        { newInitGlossary () with
                            Log =
                                { Logger.Logger.Noop with
                                    info = CA.received startupAwaiter }
                            DefaultGlossaryPathResolver = fun () -> path |> pc |> Ok |> async.Return }

                    GlossaryManager.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage startupAwaiter $"Loading contextive from {path}..."

                    let! result = GlossaryManager.lookup glossary "/some/file/anywhere" id

                    test <@ result.Count() = 1 @>

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ (foundNames, Fixtures.One.expectedTerms) ||> compareList = 0 @>
                } ] ]
