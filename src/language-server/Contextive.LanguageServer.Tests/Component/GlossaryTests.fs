module Contextive.LanguageServer.Tests.Component.GlossaryTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open System.Linq
open Tests.Helpers.SubGlossaryHelper
open Contextive.Core.File
open Contextive.LanguageServer.Tests.Helpers

module CA = ConditionAwaiter

let newTestLogger () =
    let loggedMessagesStore = ResizeArray<string>()
    let awaiter = CA.create ()

    {| loggedMessages = loggedMessagesStore
       logger =
        { Logger.info =
            fun msg ->
                loggedMessagesStore.Add(msg)
                CA.received awaiter msg
          Logger.error = fun _ -> () } |},
    awaiter


let noopMailboxProcessor () =
    MailboxProcessor.Start(fun _ -> async { return () })

let noop () = ()
let noop1 _ = ()

let newCreateClossary () =
    { Glossary.SubGlossaryOps =
        { Start = fun _ -> noopMailboxProcessor ()
          Reload = noop1 } }

let newInitGlossary () =
    { Glossary.FileScanner = fun _ -> []
      Glossary.Log = { info = noop1; error = noop1 }
      Glossary.DefaultSubGlossaryPathResolver = fun _ -> async.Return <| Error FileError.NotYetLoaded
      Glossary.RegisterWatchedFiles = fun _ _ -> noop }

[<Literal>]
let private EXPECTED_GLOSSARY_FILE_GLOB = "**/*.glossary.yml"

[<Tests>]
let tests =

    let getName (t: Contextive.Core.GlossaryFile.Term) = t.Name
    let compareList = Seq.compareWith compare

    let pc p : PathConfiguration = { Path = p; IsDefault = false }

    testList
        "LanguageServer.Glossary Tests"
        [

          testList
              "When creating a glossary"
              [ testAsync "It should start a subglossary for each discovered file" {

                    let awaiter = CA.create ()

                    let mockFileScanner glob =
                        if glob = EXPECTED_GLOSSARY_FILE_GLOB then
                            seq { "path1" }
                        else
                            Seq.empty

                    let mockStartSubGlossary (path: SubGlossary.StartSubGlossary) =
                        CA.received awaiter path.Path.Path
                        noopMailboxProcessor ()

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                SubGlossaryOps =
                                    { Start = mockStartSubGlossary
                                      Reload = noop1 } }

                    Glossary.init glossary
                    <| { newInitGlossary () with
                           FileScanner = mockFileScanner }

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
                            DefaultSubGlossaryPathResolver = fun () -> "path" |> pc |> Ok |> async.Return }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage awaiter <| Some "path"
                }

                testAsync "It should create a subglossary at the watched location" {
                    let awaiter = CA.create ()

                    let mockStartSubGlossary (path: SubGlossary.StartSubGlossary) =
                        CA.received awaiter path.Path.Path
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
                            DefaultSubGlossaryPathResolver = fun () -> "path1" |> pc |> Ok |> async.Return }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage awaiter "path1"
                }

                testAsync "It should stop watching the old file if a new file is provided" {
                    let awaiter = CA.create ()

                    let mockRegisterWatchedFiles _ _ = (fun () -> CA.received awaiter true)

                    let glossary = newCreateClossary () |> Glossary.create

                    let mutable currentPath = "path1"

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            DefaultSubGlossaryPathResolver = fun () -> currentPath |> pc |> Ok |> async.Return
                            RegisterWatchedFiles = mockRegisterWatchedFiles }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    currentPath <- "path2"

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage awaiter true
                }

                testAsync "It should log and error if unable to get the path of the configured filed" {
                    let errorAwaiter = CA.create ()

                    let glossary = newCreateClossary () |> Glossary.create

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            Log =
                                { info = noop1
                                  error = CA.received errorAwaiter }
                            DefaultSubGlossaryPathResolver =
                                fun () -> Error(FileError.PathInvalid "testing error") |> async.Return }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage errorAwaiter "Error loading glossary: Invalid Path: testing error"
                }

                ]

          testList
              "Given watched files"
              [ testList
                    "When watched file"
                    [ testAsync "is created it should start a subglossary for the watched file" {
                          let watchedFilesAwaiter = CA.create ()

                          let mockRegisterWatchedFiles watchedFilesHandlers _ =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let subGlossaryCreatedAwaiter = CA.create ()

                          let mockStartSubGlossary (path: SubGlossary.StartSubGlossary) =
                              CA.received subGlossaryCreatedAwaiter path.Path.Path
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

                          let mockRegisterWatchedFiles watchedFilesHandlers _ =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let subGlossaryReloadedAwaiter = CA.create ()

                          let subGlossary = noopMailboxProcessor ()

                          let mockStartSubGlossary _ = subGlossary

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

                          let mockRegisterWatchedFiles watchedFilesHandlers _ =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let subGlossaryReloadedAwaiter = CA.create ()

                          let subGlossary = noopMailboxProcessor ()

                          let defaultGlossaryCreatedAwaiter = CA.create ()

                          let mockStartSubGlossary (path: SubGlossary.StartSubGlossary) =
                              CA.received defaultGlossaryCreatedAwaiter path.Path.Path
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
                                  DefaultSubGlossaryPathResolver = fun () -> "pathA" |> pc |> Ok |> async.Return
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

                          let mockRegisterWatchedFiles watchedFilesHandlers _ =
                              CA.received watchedFilesAwaiter watchedFilesHandlers
                              noop

                          let subGlossaryReloadedAwaiter = CA.create ()

                          let subGlossary = noopMailboxProcessor ()

                          let defaultGlossaryCreatedAwaiter = CA.create ()

                          let mockStartSubGlossary (path: SubGlossary.StartSubGlossary) =
                              CA.received defaultGlossaryCreatedAwaiter path.Path.Path
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
                                  DefaultSubGlossaryPathResolver = fun () -> "pathA" |> pc |> Ok |> async.Return
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
              [ testAsync "when current file undefined" {
                    let awaiter = CA.create ()

                    let fileReader p =
                        CA.received awaiter p.Path

                        """contexts:
  - terms:
    - name: subGlossary1"""
                        |> Ok


                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                SubGlossaryOps =
                                    { Start = SubGlossary.start fileReader
                                      Reload = SubGlossary.reload } }

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            DefaultSubGlossaryPathResolver = fun () -> Fixtures.One.path |> pc |> Ok |> async.Return }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    let! result = Glossary.lookup glossary "" id

                    test <@ result.Count() = 1 @>

                }

                testAsync "with default subglossary only" {
                    let awaiter = CA.create ()

                    let fileReader p =
                        CA.received awaiter p.Path

                        """contexts:
  - terms:
    - name: subGlossary1"""
                        |> Ok


                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                SubGlossaryOps =
                                    { Start = SubGlossary.start fileReader
                                      Reload = SubGlossary.reload } }

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            DefaultSubGlossaryPathResolver = fun () -> Fixtures.One.path |> pc |> Ok |> async.Return }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    let! result = Glossary.lookup glossary "/some/file/anywhere" id

                    test <@ result.Count() = 1 @>

                }

                testAsync "with default and one extra subglossary" {
                    let filesMap =
                        Map
                            [ "/default/glossary.yml",
                              """contexts:
 - terms:
   - name: defaultSubGlossary"""
                              "/path2/path2.contextive.yml",
                              """contexts:
 - terms:
   - name: subGlossary2""" ]

                    let mockFileReader p = filesMap[p.Path] |> Ok

                    let mockFileScanner _ = seq { "/path2/path2.contextive.yml" }

                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                SubGlossaryOps =
                                    { Start = SubGlossary.start mockFileReader
                                      Reload = SubGlossary.reload } }

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            FileScanner = mockFileScanner
                            DefaultSubGlossaryPathResolver =
                                fun () -> "/default/glossary.yml" |> pc |> Ok |> async.Return }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    let getNames (result: Contextive.Core.GlossaryFile.FindResult) =
                        result |> Seq.collect _.Terms |> Seq.map _.Name |> Set.ofSeq

                    let! result = Glossary.lookup glossary "/path1/file.yml" id

                    test <@ result.Count() = 1 @>
                    test <@ result |> getNames |> Set.minElement = "defaultSubGlossary" @>

                    let! result = Glossary.lookup glossary "/default/pathinsamefolderasdefault.txt" id
                    test <@ result.Count() = 1 @>
                    test <@ result |> getNames |> Set.minElement = "defaultSubGlossary" @>

                    let! result = Glossary.lookup glossary "/path2/file.yml" id

                    test <@ result.Count() = 2 @>

                    let result2Terms = result |> getNames

                    let expectedTerms =
                        Set.ofSeq
                        <| seq {
                            "defaultSubGlossary"
                            "subGlossary2"
                        }

                    test <@ result2Terms = expectedTerms @>

                } ]
          testList
              "Integration"
              [ testAsync "Can collaborate with subGlossaries" {
                    let glossary =
                        Glossary.create
                            { newCreateClossary () with
                                SubGlossaryOps =
                                    { Start = SubGlossary.start FileReader.pathReader
                                      Reload = SubGlossary.reload } }

                    let startupAwaiter = CA.create ()

                    let path = Fixtures.One.path

                    Glossary.init
                        glossary
                        { newInitGlossary () with
                            Log =
                                { info = CA.received startupAwaiter
                                  error = noop1 }
                            DefaultSubGlossaryPathResolver = fun () -> path |> pc |> Ok |> async.Return }

                    Glossary.reloadDefaultGlossaryFile glossary ()

                    do! CA.expectMessage startupAwaiter $"Loading contextive from {path}..."

                    let! result = Glossary.lookup glossary "/some/file/anywhere" id

                    test <@ result.Count() = 1 @>

                    let terms = FindResult.allTerms result

                    let foundNames = terms |> Seq.map getName
                    test <@ (foundNames, Fixtures.One.expectedTerms) ||> compareList = 0 @>
                } ] ]
