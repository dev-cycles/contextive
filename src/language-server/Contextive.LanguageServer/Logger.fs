module Contextive.LanguageServer.Logger

open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Window

let DEBUG_LOG =
    System.Environment.GetEnvironmentVariable "CONTEXTIVE_DEBUG_LOG"
    |> System.String.IsNullOrEmpty
    |> not

let private noop1 _ = ()

type Logger =
    { debug: string -> unit
      info: string -> unit
      error: string -> unit }

    static member Noop =
        { debug = noop1
          info = noop1
          error = noop1 }

let forLanguageServer (s: ILanguageServer) =
    { debug =
        fun (m: string) ->
            Serilog.Log.Logger.Debug m

            if DEBUG_LOG then
                s.Window.Log m
      info =
        fun (m: string) ->
            s.Window.Log m
            Serilog.Log.Logger.Information m
      error =
        fun (m: string) ->
            s.Window.LogError m

            if s.Window.ClientSettings.Capabilities.Window.ShowMessage.IsSupported then
                s.Window.ShowWarning m

            Serilog.Log.Logger.Error m }
