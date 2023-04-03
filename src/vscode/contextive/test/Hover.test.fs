module Contextive.VsCodeExtension.Tests.Hover

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension.Tests.Helpers

let private getHoverContentValue (hoverContent : U2<MarkdownString, U2<string, {| language: string; value: string |}>>) =
    match hoverContent with
    | U2.Case1 hc -> hc.value
    | U2.Case2 hc -> 
        match hc with
        // Case1 must be second or fable defaults to just finding a string
        // And not matching the object
        | U2.Case2 hc2 -> hc2.value
        | U2.Case1 hc1 -> hc1
    |> Some

let private getHoverText (hoverResult : Hover)  =     
    Seq.tryHead hoverResult.contents
    |> Option.bind getHoverContentValue

let private getHoverTextOption = Option.bind getHoverText

let private getHover path position = promise {
    let! docUri = getDocUri path |> openDocument

    let! result = VsCodeCommands.hover docUri position

    printfn "Hover result %s %A:" path position
    logInspect result

    do! getDocUri path |> closeDocument

    return result |> Seq.map getHoverTextOption
}

let expectHoverContent content substring =
    match content with
    | Some (Some content) -> Expect.stringContains content substring "Hover should contain expected substring"
    | _ -> failwith "There should be hover content"

let tests =
    testList "Hover Tests" [

        testCaseAsync "Hover returns expected content" <| async {
            let testDocPath = "../test/fixtures/simple_workspace/.contextive/definitions.yml"
            let position = vscode.Position.Create(16, 9)

            let! hoverContents = getHover testDocPath position

            let firstHoverContent = Seq.tryHead hoverContents
                
            expectHoverContent firstHoverContent "A short summary defining the meaning of a term in a context."
        }

        testCaseAsync "Contextive Hover results appear last" <| async {
            do! updateConfig ".contextive/marketing.yml"

            let testDocPath = "../test/fixtures/simple_workspace/MarketingDemo.cs"
            let position = vscode.Position.Create(0, 15)

            let! hoverContents = getHover testDocPath position

            Expect.hasLength hoverContents 2 "Should have 2 hover results"

            // printfn "hoverContents: %A" hoverContents

            let firstHoverContent = Seq.tryHead hoverContents
            let secondHoverContent = hoverContents |> Seq.tail |> Seq.tryHead

            // The commented lines are what is really expected. The uncommented lines are what is happening.
            // See https://github.com/microsoft/vscode/issues/178184 for why
            // Notwithstanding inability to test, this is working properly at runtime.
            // Leaving the incorrect assertions in place so that we find out when the bug is fixed.
            
            // expectHoverContent firstHoverContent "class Page"
            // expectHoverContent secondHoverContent "All the content displayed in a browser when a user visits a url."
            expectHoverContent secondHoverContent "class Page"
            expectHoverContent firstHoverContent "All the content displayed in a browser when a user visits a url."
        }
    ]