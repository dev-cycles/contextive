module Contextive.VsCodeExtension.Initialize

open Fable.Import.LanguageServer
open Fable.Core

let handler (client: LanguageClient) _ : obj option =
    promise {
            do! Waiter.waitFor <|
                fun () -> promise {
                    try
                        do! client.sendRequest ("contextive/initialize", System.Threading.CancellationToken.None)
                        return true
                    with
                    // Sometimes the combination of file content changes, opening document, and reloading settings,
                    // causes a "Content Modified" error. It is benign from a UX perspective.
                    | ex when ex.Message.Contains("Method not found - contextive/initialize") -> return false
                    | ex when ex.Message <> "Content Modified" ->
                        raise ex
                        return true
                    | _ -> return true
                }
    }
    :> obj
    |> Some
