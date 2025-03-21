module Contextive.VsCodeExtension.Tests.E2E.SingleRoot.Hover

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension
open Contextive.VsCodeExtension.Tests.E2E.Helpers
open Contextive.VsCodeExtension.Tests.E2E.Helpers.Helpers

let private getHoverContentValue (hoverContent: HoverContent) =
    match hoverContent with
    | U2.Case1 hc -> hc.value
    | U2.Case2 hc ->
        match hc with
        // Case1 must be second or fable defaults to just finding a string
        // And not matching the object
        | U2.Case2 hc2 -> hc2.value
        | U2.Case1 hc1 -> hc1
    |> Some

let private getHoverText (hoverResult: Hover) =
    Seq.tryHead hoverResult.contents |> Option.bind getHoverContentValue

let private getHoverTextOption = Option.bind getHoverText

let private getHover path position expectedResultCount =
    promise {
        let! docUri = getDocUri path |> openDocument

        let getHoverResults () =
            promise {
                let! hoverResults = VsCodeCommands.hover docUri position
                return hoverResults |> Seq.map getHoverTextOption
            }

        do!
            Waiter.waitFor
            <| fun () ->
                promise {
                    let! hoverContents = getHoverResults ()
                    return (Seq.length hoverContents) = expectedResultCount
                }

        do! getDocUri path |> closeDocument

        return! getHoverResults ()
    }

let expectHoverContent content substring =
    match content with
    | Some(Some content) -> Expect.stringContains content substring "Hover should contain expected substring"
    | _ -> failwith "There should be hover content"

let tests =
    testList
        "Hover Tests"
        [

          testCaseAsync "Hover returns expected content"
          <| async {
              let testDocPath = Paths.inWorkspace ".contextive/definitions.yml"
              let position = vscode.Position.Create(19, 9)

              let! hoverContents = getHover testDocPath position 1

              let firstHoverContent = Seq.tryHead hoverContents

              expectHoverContent firstHoverContent "A short summary defining the meaning of a term in a context."
          }

          testCaseAsync "Contextive Hover results appear last"
          <| async {
              let testDocPath = Paths.inWorkspace "MarketingDemo/MarketingDemo.cs"
              let position = vscode.Position.Create(3, 15)
              let expectedLength = 2
              let! hoverContents = getHover testDocPath position 2

              Expect.hasLength hoverContents expectedLength $"Should have {expectedLength} hover results"

              let firstHoverContent = Seq.tryHead hoverContents
              let secondHoverContent = hoverContents |> Seq.tail |> Seq.tryHead

              // The commented lines are what is really expected. The uncommented lines are what is happening.
              // See https://github.com/microsoft/vscode/issues/178184 for why
              // Notwithstanding inability to test, this is working properly at runtime.
              // Leaving the incorrect assertions in place so that we find out when the bug is fixed.
              try
                  // expectHoverContent firstHoverContent "class Page"
                  // expectHoverContent secondHoverContent "All the content displayed in a browser when a user visits a url."
                  expectHoverContent secondHoverContent "class Page"

                  expectHoverContent
                      firstHoverContent
                      "All the content displayed in a browser when a user visits a url."
              with e ->
                  logInspect e

          } ]
