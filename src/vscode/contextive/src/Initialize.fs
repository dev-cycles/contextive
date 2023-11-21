module Contextive.VsCodeExtension.Initialize

open Fable.Import.LanguageServer


let handler (client: LanguageClient) _ : obj option =
    promise {
        try
            do! client.sendRequest ("contextive/initialize", System.Threading.CancellationToken.None)
        with
        // Sometimes the combination of file content changes, opening document, and reloading settings,
        // causes a "Content Modified" error. It is benign from a UX perspective.
        | ex when ex.Message <> "Content Modified" -> raise ex
        | _ -> ()
    }
    :> obj
    |> Some
