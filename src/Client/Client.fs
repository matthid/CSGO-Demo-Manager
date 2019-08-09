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
    DemoView : DemosView.Model }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| DemoViewMsg of msg:DemosView.Msg
| StartDownloadMM


    //Cmd.OfPromise.perform fetch () (fun d -> NewData(startItem, d))
// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let demoInit, demoCmd = DemosView.init()
    let initialModel = { DemoView = demoInit }
    initialModel, demoCmd |> Cmd.map DemoViewMsg

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | DemoViewMsg msg ->
        let demoUpdate, demoCmd = DemosView.update msg currentModel.DemoView
        { currentModel with DemoView = demoUpdate }, demoCmd |> Cmd.map DemoViewMsg
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
        Columns.columns [] [
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
