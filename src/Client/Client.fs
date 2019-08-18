module Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fetch.Types
open Thoth.Fetch
open Fulma
open Thoth.Json
open Fable.Core
open Fable.Core.JsInterop

open Shared


// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = {
    DemoView : DemosView.Model
    NotificationsView : NotificationsView.Model }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| DemoViewMsg of msg:DemosView.Msg
| NotificationViewMsg of msg:NotificationsView.Msg
| StartDownloadMM
| UpdateState
| StartDownloadMMStarted of res:StartMMDownloadResult

let retryTimeout = 5000
let signalR =
    SignalR.signalr.HubConnectionBuilder.Create()
        .withUrl(Backend.getUrl "/socket/notifications")
        .configureLogging(SignalR.LogLevel.Information)
        .build()
let startSignalRConnection (connection:SignalR.HubConnection) (onConnected: bool -> unit) =
    let rec retry () =
        promise {
            try
                do! connection.start()
                JS.console.log("[SignalR] connected!")
                onConnected true
            with e ->
                JS.console.error("[SignalR] connection failed!", e)
                do! Promise.sleep retryTimeout
                do! retry()
        }
    let onClose (err:exn) =
        promise {
            JS.console.log("[SignalR] connection lost!", err)
            onConnected false
            do! Promise.sleep retryTimeout
            do! retry()
        }
        |> Promise.start
    
    connection.onclose onClose
    Promise.start(retry())

    //Cmd.OfPromise.perform fetch () (fun d -> NewData(startItem, d))
// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let demoInit, demoCmd = DemosView.init()
    let notificationInit, notificationCmd = NotificationsView.init()
    let initialModel = { DemoView = demoInit; NotificationsView = notificationInit }

    let signalRSub = Cmd.ofSub(fun dispatch -> startSignalRConnection signalR (fun b -> 
        // dispatch Connected/Disconnected if needed
        if b then
            dispatch UpdateState
        ignore ()
    ))

    let signalREvents = Cmd.ofSub(fun dispatch ->
        signalR.on("Notification", fun args ->
            let json = string args
            let notification = Thoth.Json.Decode.Auto.unsafeFromString json
            dispatch(NotificationViewMsg <| NotificationsView.Notification notification))
    )

    initialModel, Cmd.batch [ demoCmd |> Cmd.map DemoViewMsg; signalRSub; signalREvents ]

let startDownloadMM () =
    let url = sprintf "/api/downloadMM"
    Fetch.fetchAs<StartMMDownloadResult> (
        Backend.getUrl url, 
        [ Method HttpMethod.POST; RequestProperties.Body (BodyInit.Case2 "test") ] )

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | DemoViewMsg msg ->
        let demoUpdate, demoCmd = DemosView.update msg currentModel.DemoView
        { currentModel with DemoView = demoUpdate }, demoCmd |> Cmd.map DemoViewMsg
    | NotificationViewMsg msg ->
        let notificationUpdate, notificationCmd = NotificationsView.update msg currentModel.NotificationsView
        { currentModel with NotificationsView = notificationUpdate }, notificationCmd |> Cmd.map NotificationViewMsg
    | StartDownloadMM ->
        currentModel, Cmd.OfPromise.result (startDownloadMM () |> Promise.map StartDownloadMMStarted)
    | UpdateState ->
        currentModel, Cmd.ofMsg (NotificationViewMsg NotificationsView.UpdateTasks)
    | _ -> currentModel, Cmd.none


let safeComponents =
    let components =
        span [ ]
           [ a [ Href "https://github.com/SAFE-Stack/SAFE-template" ]
               [ str "SAFE  "
                 str Version.template ]
             str ", "
             a [ Href "https://github.com/giraffe-fsharp/Giraffe" ] [ str "Giraffe" ]
             str ", "
             a [ Href "http://fable.io" ] [ str "Fable" ]
             str ", "
             a [ Href "https://elmish.github.io" ] [ str "Elmish" ]
             str ", "
             a [ Href "https://fulma.github.io/Fulma" ] [ str "Fulma" ]

           ]

    span [ ]
        [ str "Version "
          strong [ ] [ str Version.app ]
          str " powered by: "
          components ]

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]
        

let buttons model dispatch =

    Container.container [] [
        Columns.columns [ Columns.Option.Props [ Style [ Margin "0px" ] ] ] [
            Column.column [] [ button "Analyse" (fun _ -> ()) ]
            Column.column [] [ button "Download MM Demos" (fun _ -> dispatch StartDownloadMM) ]
            Column.column [] [ button "Watch" (fun _ -> ()) ]
        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "CSGO Demo Manager" ] ] ]
          
          NotificationsView.view model.NotificationsView (NotificationViewMsg >> dispatch)
          buttons model dispatch
          DemosView.view model.DemoView (DemoViewMsg >> dispatch)

          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ] ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
