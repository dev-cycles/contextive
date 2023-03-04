module Contextive.VsCodeExtension.Tests.Hover

open Fable.Mocha
open Fable.Core
open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode
open Contextive.VsCodeExtension.Tests.Helpers

let private getHoverContentValue (hoverContent : U2<MarkdownString, MarkedString>) =
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

    do! getDocUri path |> closeDocument

    return result |> Seq.map getHoverTextOption
}

let tests =
    testList "Hover Tests" [

        testCaseAsync "Hover returns expected content" <| async {
            let testDocPath = "../test/fixtures/simple_workspace/.contextive/definitions.yml"
            let position = vscode.Position.Create(16, 9)

            let! hoverContents = getHover testDocPath position

            let firstHoverContent = Seq.tryHead hoverContents
                
            match firstHoverContent with
            | Some (Some content) -> Expect.stringContains content "A short summary defining the meaning of a term in a context." "Hover should contain description"
            | _ -> failwith "There should be some "
        }
    ]