module Contextive.LanguageServer.Configuration

open System.Threading.Tasks

let resolvedPathGetter configGetter pathResolver () =
    async {
        let! path = configGetter ()
        return pathResolver path
    }

let handler (glossaryFileReloader: SubGlossary.Reloader) _ =
    glossaryFileReloader ()
    Task.CompletedTask
