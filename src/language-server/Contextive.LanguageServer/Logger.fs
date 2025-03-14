module Contextive.LanguageServer.Logger

open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Window

let private noop1 _ = ()

type Logger =
    { info: string -> unit
      error: string -> unit }

    static member Noop = { info = noop1; error = noop1 }

let forLanguageServer (s: ILanguageServer) =
    { info =
        fun (m: string) ->
            s.Window.Log(m)
            Serilog.Log.Logger.Information(m)
      error =
        fun (m: string) ->
            s.Window.LogError(m)
            s.Window.ShowWarning(m)
            Serilog.Log.Logger.Error(m) }
