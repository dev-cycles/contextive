module Contextive.LanguageServer.Logger

type Logger =
    { info: string -> unit }

    static member Noop = { info = fun _ -> () }
