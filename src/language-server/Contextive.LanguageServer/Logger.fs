module Contextive.LanguageServer.Logger

open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Window

type Logger =
    { info: string -> unit }

    static member Noop = { info = fun _ -> () }

let forLanguageServer (s: ILanguageServer) =
    { info =
        fun (m: string) ->
            s.Window.Log(m)
            Serilog.Log.Logger.Information(m) }
