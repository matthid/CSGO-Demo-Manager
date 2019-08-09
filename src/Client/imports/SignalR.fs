// ts2fable 0.6.1
module rec SignalR

open System
open Fable.Core
open Fable.Import.JS

let [<Import("*","@aspnet/signalr")>] signalr: SignalR.IExports = jsNative

type [<AllowNullLiteral>] IExports =
    abstract HubConnectionBuilder: HubConnectionBuilderStatic

type [<RequireQualifiedAccess>] HttpTransportType =
| None = 0
| WebSockets = 1
| ServerSentEvents = 2
| LongPolling = 4

/// Represents a connection to a SignalR Hub.
type [<AllowNullLiteral>] HubConnection =
    /// The server timeout in milliseconds.
    ///
    /// If this timeout elapses without receiving any messages from the server, the connection will be terminated with an error.
    /// The default timeout value is 30,000 milliseconds (30 seconds).
    abstract serverTimeoutInMilliseconds: float with get, set
    /// Starts the connection.
    abstract start: unit -> Promise<unit>
    /// Stops the connection.
    abstract stop: unit -> Promise<unit>
    /// <summary>Invokes a streaming hub method on the server using the specified name and arguments.</summary>
    /// <param name="methodName">The name of the server method to invoke.</param>
    /// <param name="args">The arguments used to invoke the server method.</param>
    abstract send: methodName: string * args : obj -> Promise<unit>
    /// <summary>Invokes a hub method on the server using the specified name and arguments.
    ///
    /// The Promise returned by this method resolves when the server indicates it has finished invoking the method. When the promise
    /// resolves, the server has finished invoking the method. If the server method returns a result, it is produced as the result of
    /// resolving the Promise.</summary>
    /// <param name="methodName">The name of the server method to invoke.</param>
    /// <param name="args">The arguments used to invoke the server method.</param>
    abstract invoke: methodName: string * args: obj -> Promise<'T>
    /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
    /// <param name="methodName">The name of the hub method to define.</param>
    /// <param name="newMethod">The handler that will be raised when the hub method is invoked.</param>
    abstract on: methodName: string * newMethod: (obj -> unit) -> unit
    /// <summary>Removes all handlers for the specified hub method.</summary>
    /// <param name="methodName">The name of the method to remove handlers for.</param>
    abstract off: methodName: string -> unit
    /// <summary>Removes the specified handler for the specified hub method.
    ///
    /// You must pass the exact same Function instance as was previously passed to {@link on}. Passing a different instance (even if the function
    /// body is the same) will not remove the handler.</summary>
    /// <param name="methodName">The name of the method to remove handlers for.</param>
    /// <param name="method">The handler to remove. This must be the same Function instance as the one passed to {</param>
    abstract off: methodName: string * ``method``: (ResizeArray<obj option> -> unit) -> unit
    abstract off: methodName: string * ?``method``: (ResizeArray<obj option> -> unit) -> unit
    /// <summary>Registers a handler that will be invoked when the connection is closed.</summary>
    /// <param name="callback">The handler that will be invoked when the connection is closed. Optionally receives a single argument containing the error that caused the connection to close (if any).</param>
    abstract onclose: callback: (Error -> unit) -> unit

/// A builder for configuring {@link HubConnection} instances.
type [<AllowNullLiteral>] HubConnectionBuilder =

    /// <summary>Configures console logging for the {@link HubConnection}.</summary>
    /// <param name="logLevel">The minimum level of messages to log. Anything at this level, or a more severe level, will be logged.</param>
    abstract configureLogging: logLevel: LogLevel -> HubConnectionBuilder
    /// <summary>Configures custom logging for the {@link HubConnection}.</summary>
    /// <param name="logger">An object implementing the {</param>
    abstract configureLogging: logger: ILogger -> HubConnectionBuilder
    abstract configureLogging: logging: U2<LogLevel, ILogger> -> HubConnectionBuilder
    /// <summary>Configures the {@link HubConnection} to use HTTP-based transports to connect to the specified URL.
    ///
    /// The transport will be selected automatically based on what the server and client support.</summary>
    /// <param name="url">The URL the connection will use.</param>
    abstract withUrl: url: string -> HubConnectionBuilder

    /// <summary>Configures the {@link HubConnection} to use the specified HTTP-based transport to connect to the specified URL.</summary>
    /// <param name="url">The URL the connection will use.</param>
    /// <param name="transportType">The specific transport to use.</param>
    abstract withUrl: url: string * transportType: HttpTransportType -> HubConnectionBuilder

    /// <summary>Configures the {@link HubConnection} to use HTTP-based transports to connect to the specified URL.</summary>
    /// <param name="url">The URL the connection will use.</param>
    /// <param name="options"> An options object used to configure the connection.</param>
    abstract withUrl: url: string * options: IHttpConnectionOptions -> HubConnectionBuilder

    /// Creates a {@link HubConnection} from the configuration options specified in this builder.
    abstract build: unit -> HubConnection

/// A builder for configuring {@link HubConnection} instances.
type [<AllowNullLiteral>] HubConnectionBuilderStatic =
    [<Emit "new $0($1...)">] abstract Create: unit -> HubConnectionBuilder

/// Options provided to the 'withUrl' method on {@link HubConnectionBuilder} to configure options for the HTTP-based transports.
type IHttpConnectionOptions = {
    /// A function that provides an access token required for HTTP Bearer authentication.
    accessTokenFactory: unit -> string
}

type [<RequireQualifiedAccess>] LogLevel =
    | Trace = 0
    | Debug = 1
    | Information = 2
    | Warning = 3
    | Error = 4
    | Critical = 5
    | None = 6

/// An abstraction that provides a sink for diagnostic messages.
type [<AllowNullLiteral>] ILogger =
    /// <summary>Called by the framework to emit a diagnostic message.</summary>
    /// <param name="logLevel">The severity level of the message.</param>
    /// <param name="message">The message.</param>
    abstract log: logLevel: LogLevel * message: string -> unit
