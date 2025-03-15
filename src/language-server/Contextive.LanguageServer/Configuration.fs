module Contextive.LanguageServer.Configuration

open System.Threading.Tasks

let resolvedPathGetter configGetter pathResolver () =
    async {
        let! path = configGetter ()
        return pathResolver path
    }

let handler onDefaultGlossaryLocationChanged _ =
    onDefaultGlossaryLocationChanged ()
    Task.CompletedTask
