// ts2fable 0.0.0
namespace Fable.Import

open System
open Fable.Core
open Fable.Core.JS
open Fable.Import.VSCode.Vscode
open Node.ChildProcess

module rec LanguageServer =

    // type Error = System.Exception

    // ts2fable 0.0.0
    module Client =

        type Message = obj
        type RPCMessageType = obj
        type ResponseError = obj
        type RequestType = obj
        type RequestType0 = obj
        type RequestHandler = obj
        type RequestHandler0 = obj
        type GenericRequestHandler = obj
        type NotificationType = obj
        type NotificationHandler<'a> = 'a -> unit
        type GenericNotificationHandler = obj
        type MessageReader = obj
        type MessageWriter = obj
        type Trace = obj
        type Event = obj
        type ClientCapabilities = obj
        type TextDocumentRegistrationOptions = obj
        type InitializeParams = obj
        type InitializeResult = obj
        type InitializeError = obj
        type ServerCapabilities = obj
        type ColorProviderMiddleware = obj
        type ImplementationMiddleware = obj
        type TypeDefinitionMiddleware = obj
        type ConfigurationWorkspaceMiddleware = obj
        type WorkspaceFolderWorkspaceMiddleware = obj
        type FoldingRangeProviderMiddleware = obj
        type DeclarationMiddleware = obj
        type SelectionRangeProviderMiddleware = obj
        type TextDocumentFeatureStatic = obj

        [<RequireQualifiedAccess>]
        type ErrorAction =
            | Continue = 1
            | Shutdown = 2

        [<RequireQualifiedAccess>]
        type CloseAction =
            | DoNotRestart = 1
            | Restart = 2

        /// A pluggable error handler that is invoked when the connection is either
        /// producing errors or got closed.
        [<AllowNullLiteral>]
        type ErrorHandler =
            /// <summary>An error has occurred while writing or reading from the connection.</summary>
            /// <param name="error">- the error received</param>
            /// <param name="message">- the message to be delivered to the server if know.</param>
            /// <param name="count">- a count indicating how often an error is received. Will
            /// be reset if a message got successfully send or received.</param>
            abstract error: error: Exception * message: Message * count: float -> ErrorAction
            /// The connection to the server got closed.
            abstract closed: unit -> CloseAction

        /// A handler that is invoked when the initialization of the server failed.
        [<AllowNullLiteral>]
        type InitializationFailedHandler =
            /// <param name="error">The error returned from the server</param>
            [<Emit "$0($1...)">]
            abstract Invoke: error: U3<ResponseError, Exception, obj option> -> bool

        [<AllowNullLiteral>]
        type SynchronizeOptions =
            /// The configuration sections to synchronize. Pushing settings from the
            /// client to the server is deprecated in favour of the new pull model
            /// that allows servers to query settings scoped on resources. In this
            /// model the client can only deliver an empty change event since the
            /// actually setting value can vary on the provided resource scope.
            abstract configurationSection: U2<string, ResizeArray<string>> option with get, set
            abstract fileEvents: U2<FileSystemWatcher, ResizeArray<FileSystemWatcher>> option with get, set

        [<RequireQualifiedAccess>]
        type RevealOutputChannelOn =
            | Info = 1
            | Warn = 2
            | Error = 3
            | Never = 4

        [<AllowNullLiteral>]
        type Middleware =
            interface
            end

        [<AllowNullLiteral>]
        type LanguageClientOptions =
            abstract documentSelector: DocumentSelector option with get, set
            abstract synchronize: SynchronizeOptions option with get, set
            abstract diagnosticCollectionName: string option with get, set
            abstract outputChannel: OutputChannel option with get, set
            abstract outputChannelName: string option with get, set
            abstract traceOutputChannel: OutputChannel option with get, set
            abstract revealOutputChannelOn: RevealOutputChannelOn option with get, set
            /// The encoding use to read stdout and stderr. Defaults
            /// to 'utf8' if ommitted.
            abstract stdioEncoding: string option with get, set
            abstract initializationOptions: U2<obj option, (unit -> obj option)> option with get, set
            abstract initializationFailedHandler: InitializationFailedHandler option with get, set
            abstract errorHandler: ErrorHandler option with get, set
            abstract middleware: Middleware option with get, set

        [<RequireQualifiedAccess>]
        type State =
            | Stopped = 1
            | Starting = 3
            | Running = 2

        [<AllowNullLiteral>]
        type StateChangeEvent =
            abstract oldState: State with get, set
            abstract newState: State with get, set

        [<AllowNullLiteral>]
        type RegistrationData<'T> =
            abstract id: string with get, set
            abstract registerOptions: 'T with get, set

        /// A static feature. A static feature can't be dynamically activate via the
        /// server. It is wired during the initialize sequence.
        [<AllowNullLiteral>]
        type StaticFeature =
            /// Called to fill the initialize params.
            abstract fillInitializeParams: (InitializeParams -> unit) option with get, set
            /// <summary>Called to fill in the client capabilities this feature implements.</summary>
            /// <param name="capabilities">The client capabilities to fill.</param>
            abstract fillClientCapabilities: capabilities: ClientCapabilities -> unit
            /// <summary>Initialize the feature. This method is called on a feature instance
            /// when the client has successfully received the initialize request from
            /// the server and before the client sends the initialized notification
            /// to the server.</summary>
            /// <param name="capabilities">the server capabilities</param>
            /// <param name="documentSelector">the document selector pass to the client's constructor.
            /// May be `undefined` if the client was created without a selector.</param>
            abstract initialize: capabilities: ServerCapabilities * documentSelector: DocumentSelector option -> unit

        [<AllowNullLiteral>]
        type DynamicFeature<'T> =
            /// The message for which this features support dynamic activation / registration.
            abstract messages: U2<RPCMessageType, ResizeArray<RPCMessageType>> with get, set
            /// Called to fill the initialize params.
            abstract fillInitializeParams: (InitializeParams -> unit) option with get, set
            /// <summary>Called to fill in the client capabilities this feature implements.</summary>
            /// <param name="capabilities">The client capabilities to fill.</param>
            abstract fillClientCapabilities: capabilities: ClientCapabilities -> unit
            /// <summary>Initialize the feature. This method is called on a feature instance
            /// when the client has successfully received the initalize request from
            /// the server and before the client sends the initialized notification
            /// to the server.</summary>
            /// <param name="capabilities">the server capabilities.</param>
            /// <param name="documentSelector">the document selector pass to the client's constuctor.
            /// May be `undefined` if the client was created without a selector.</param>
            abstract initialize: capabilities: ServerCapabilities * documentSelector: DocumentSelector option -> unit
            /// <summary>Is called when the server send a register request for the given message.</summary>
            /// <param name="message">the message to register for.</param>
            /// <param name="data">additional registration data as defined in the protocol.</param>
            abstract register: message: RPCMessageType * data: RegistrationData<'T> -> unit
            /// <summary>Is called when the server wants to unregister a feature.</summary>
            /// <param name="id">the id used when registering the feature.</param>
            abstract unregister: id: string -> unit
            /// Called when the client is stopped to dispose this feature. Usually a feature
            /// unregisters listeners registerd hooked up with the VS Code extension host.
            abstract dispose: unit -> unit


        [<AllowNullLiteral>]
        type MessageTransports =
            abstract reader: MessageReader with get, set
            abstract writer: MessageWriter with get, set
            abstract detached: bool option with get, set

        module MessageTransports =

            [<AllowNullLiteral>]
            type IExports =
                abstract is: value: obj option -> bool

        [<AllowNullLiteral>]
        type BaseLanguageClient =
            abstract initializeResult: InitializeResult option
            abstract sendRequest: ``type``: RequestType0 * ?token: CancellationToken -> Promise<'R>
            abstract sendRequest: ``type``: RequestType * ``params``: 'P * ?token: CancellationToken -> Promise<'R>
            abstract sendRequest: ``method``: string * ?token: CancellationToken -> Promise<'R>
            abstract sendRequest: ``method``: string * param: obj option * ?token: CancellationToken -> Promise<'R>
            abstract onRequest: ``type``: RequestType0 * handler: RequestHandler0 -> unit
            abstract onRequest: ``method``: string * handler: GenericRequestHandler -> unit
            abstract sendNotification: ``type``: NotificationHandler<'a> -> unit
            abstract sendNotification: ``type``: NotificationType * ?``params``: 'P -> unit
            abstract sendNotification: ``method``: string -> unit
            abstract sendNotification: ``method``: string * ``params``: obj option -> unit
            abstract onNotification: ``type``: NotificationType * handler: NotificationHandler<'a> -> unit
            abstract onNotification: ``method``: string * handler: NotificationHandler<'a> -> unit
            abstract clientOptions: LanguageClientOptions
            abstract onTelemetry: Event<obj option>
            abstract onDidChangeState: Event<StateChangeEvent>
            abstract outputChannel: OutputChannel
            abstract traceOutputChannel: OutputChannel
            abstract diagnostics: DiagnosticCollection option
            abstract createDefaultErrorHandler: unit -> ErrorHandler
            abstract trace: Trace with get, set
            abstract info: message: string * ?data: obj -> unit
            abstract warn: message: string * ?data: obj -> unit
            abstract error: message: string * ?data: obj -> unit
            abstract needsStart: unit -> bool
            abstract needsStop: unit -> bool
            abstract onReady: unit -> Promise<unit>
            abstract start: unit -> Disposable
            abstract stop: unit -> Promise<unit>
            abstract createMessageTransports: encoding: string -> Promise<MessageTransports>
            abstract handleConnectionClosed: unit -> unit
            abstract registerFeatures: features: ResizeArray<U2<StaticFeature, DynamicFeature<obj option>>> -> unit
            abstract registerFeature: feature: U2<StaticFeature, DynamicFeature<obj option>> -> unit
            abstract registerBuiltinFeatures: unit -> unit
            abstract logFailedRequest: ``type``: RPCMessageType * error: obj option -> unit

    [<AllowNullLiteral>]
    type ExecutableOptions =
        abstract cwd: string option with get, set
        abstract stdio: U2<string, ResizeArray<string>> option with get, set
        abstract env: obj option with get, set
        abstract detached: bool option with get, set
        abstract shell: bool option with get, set

    [<AllowNullLiteral>]
    type Executable =
        abstract command: string with get, set
        abstract args: ResizeArray<string> option with get, set
        abstract options: ExecutableOptions option with get, set

    [<AllowNullLiteral>]
    type ForkOptions =
        abstract cwd: string option with get, set
        abstract env: obj option with get, set
        abstract encoding: string option with get, set
        abstract execArgv: ResizeArray<string> option with get, set

    [<RequireQualifiedAccess>]
    type TransportKind =
        | Stdio = 0
        | Ipc = 1
        | Pipe = 2
        | Socket = 3

    [<AllowNullLiteral>]
    type SocketTransport =
        abstract kind: TransportKind with get, set
        abstract port: float with get, set

    type Transport = U2<TransportKind, SocketTransport>

    [<AllowNullLiteral>]
    type NodeModule =
        abstract ``module``: string with get, set
        abstract transport: Transport option with get, set
        abstract args: ResizeArray<string> option with get, set
        abstract runtime: string option with get, set
        abstract options: ForkOptions option with get, set

    [<AllowNullLiteral>]
    type StreamInfo =
        abstract writer: obj with get, set
        abstract reader: obj with get, set
        abstract detached: bool option with get, set

    [<AllowNullLiteral>]
    type ChildProcessInfo =
        abstract ``process``: ChildProcess with get, set
        abstract detached: bool with get, set

    type ServerOptions =
        U7<Executable, obj, obj, obj, obj, NodeModule, (unit
            -> Promise<U4<ChildProcess, StreamInfo, Client.MessageTransports, ChildProcessInfo>>)>

    [<AllowNullLiteral>]
    [<Import("LanguageClient", "vscode-languageclient")>]
    type LanguageClient
        (
            id: string,
            name: string,
            serverOptions: ServerOptions,
            clientOptions: Client.LanguageClientOptions,
            forceDebug: bool
        ) =
        //Inharited from BaseClient
        member __.initializeResult: Client.InitializeResult option = jsNative

        member __.sendRequest<'P, 'R>
            (
                ``type``: Client.RequestType,
                ``params``: 'P,
                ?token: CancellationToken
            ) : Promise<'R> =
            jsNative

        member __.onRequest(``type``: Client.RequestType, handler: Client.RequestHandler) : unit = jsNative
        member __.sendNotification(``type``: Client.NotificationType, ?params: 'P) : unit = jsNative

        member __.onNotification(``type``: Client.NotificationType, handler: Client.NotificationHandler<'a>) : unit =
            jsNative

        member __.onNotification(``method``: string, handler: Client.NotificationHandler<'a>) : unit = jsNative
        member __.clientOptions: Client.LanguageClientOptions = jsNative
        member __.onTelemetry: Event<obj option> = jsNative
        member __.onDidChangeState: Event<Client.StateChangeEvent> = jsNative
        member __.outputChannel: OutputChannel = jsNative
        member __.traceOutputChannel: OutputChannel = jsNative
        member __.diagnostics: DiagnosticCollection option = jsNative
        member __.createDefaultErrorHandler() : Client.ErrorHandler = jsNative
        member __.info(message: string, ?data: obj) : unit = jsNative
        member __.warn(message: string, ?data: obj) : unit = jsNative
        member __.error(message: string, ?data: obj) : unit = jsNative
        member __.needsStart() : bool = jsNative
        member __.needsStop() : bool = jsNative
        member __.onReady() : Promise<unit> = jsNative
        member __.start() : Disposable = jsNative
        member __.logFailedRequest(``type``: Client.RPCMessageType, error: obj option) : unit = jsNative

        //LanguageClient
        member __.stop() : Promise<unit> = jsNative
        member __.handleConnectionClosed() : unit = jsNative
        member __.createMessageTransports(encoding: string) : Promise<Client.MessageTransports> = jsNative
        member __.registerProposedFeatures() : unit = jsNative
        member __.registerBuiltinFeatures() : unit = jsNative
