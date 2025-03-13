module Contextive.LanguageServer.Configuration

open System.Threading.Tasks

let resolvedPathGetter configGetter pathResolver () =
    async {
        let! path = configGetter ()
        return pathResolver path
    }

let handler (onDefaultGlossaryLocationChanged: SubGlossary.OnChangedHandler) _ =
    onDefaultGlossaryLocationChanged ()
    Task.CompletedTask
