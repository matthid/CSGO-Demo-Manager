module Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fetch.Types
open Thoth.Fetch
open Fulma
open Thoth.Json

open Shared

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = {
    LoadingData : bool
    Pages : int
    SelectedDemos : string list
    CurrentStart: int
    CurrentPageSize: int
    CurrentData: Demo list }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| LoadingData of start:int * pageSize:int
| NewData of start:int * pageSize:int * DemoData
| SelectionChanged of string list

let fetchDemos (startItem:int) (maxItems:int) =
    let url = sprintf "/api/demos?startItem=%d&maxItems=%d" startItem maxItems
    Fetch.fetchAs<DemoData> (Backend.getUrl url)
    |> Promise.map (fun d -> startItem, maxItems, d)

    //Cmd.OfPromise.perform fetch () (fun d -> NewData(startItem, d))
// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = { CurrentData = []; LoadingData = true; Pages = 1; SelectedDemos = []; CurrentStart = 0; CurrentPageSize = 20 }
    let loadCountCmd = Cmd.OfPromise.result (fetchDemos 0 20 |> Promise.map NewData)
    initialModel, loadCountCmd

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | LoadingData (start, pageSize) when not currentModel.LoadingData ->
        let nextModel = { currentModel with LoadingData = true; CurrentPageSize = pageSize; CurrentStart = start }
        
        nextModel, Cmd.OfPromise.result (fetchDemos start pageSize |> Promise.map NewData)
    | SelectionChanged sel ->
        let nextModel = { currentModel with SelectedDemos = sel }
        nextModel, Cmd.none
    | NewData (start, pageSize, data) ->
        let nextModel = { currentModel with CurrentData = data.Demos; LoadingData = false; Pages = data.Pages; SelectedDemos = []; CurrentStart = start; CurrentPageSize = pageSize }
        nextModel, Cmd.none
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

let demoView (demo:Demo) =
    ReactTable.table
open Fable.Core
open Fable.Core.JsInterop
let inline (~%) x = createObj x
let inline (=>) k v = k ==> v
    
type Person =
    { name: string; age: int; friend: Person option }
    
let data =
    [| { name = "Tanner Linsley"
         age = 26
         friend = Some { name = "Jason Maurer"
                         age = 23
                         friend = None } } |]
    |> ResizeArray

let columns =
    let c1 = createEmpty<ReactTable.Column<Demo, string>>
    c1.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "Name")
    c1.id <- Some "Name"
    c1.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.Name))
    let c2 = createEmpty<ReactTable.Column<Demo, string>>
    c2.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "Comment")
    c2.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.Comment))
    c2.id <- Some "Comment"
    c2.Cell <- Some (ReactTable.TableCellRenderer.ofCase1 (fun info -> span [] [str (string info.value)]))
    let c3 = createEmpty<ReactTable.Column<Demo, string>>
    c3.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "Map")
    c3.id <- Some "Map"
    c3.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.MapName))
    [|c1 :?> ReactTable.Column<Demo> ; c2 :?> _; c3 :?> _|]

let demosView (model:Model) dispatch =
    let p = createEmpty<ReactTable.SelectTableProps<Demo, Demo>>
    p.data <- model.CurrentData |> ResizeArray
    p.columns <- Some columns
    p.loading <- model.LoadingData
    p.pages <- Some (float model.Pages)
    p.page <- Some (float model.CurrentStart / float model.CurrentPageSize)
    p.pageSize <- Some (float model.CurrentPageSize)
    p.manual <- true
    p.onFetchData <- (fun state instance ->
        async {
            let newStart = int state.pageSize * int state.page
            let newPageSize = int state.pageSize
            if model.LoadingData || newStart = model.CurrentStart && newPageSize = model.CurrentPageSize then
                ()
            else
                dispatch(LoadingData(newStart, newPageSize))
        }
        |> Async.StartImmediate

    )
    let toogle key =
        let current =
            model.SelectedDemos
            |> List.filter (fun demKey -> demKey <> key)
        let newSelected = 
            if current.Length < model.SelectedDemos.Length then
                current
            else
                key :: current
        
        dispatch (SelectionChanged newSelected)
    p.toggleSelection <- Some (fun key shift _row ->
        toogle key
    )

    let isSelected key = model.SelectedDemos |> Seq.contains key
    p.isSelected <- Some (isSelected)

    p.getTrProps <- U2.Case1 (ReactTable.ComponentPropsGetterR(fun state rowInfo column instance ->
        let id, isSelected =
            match rowInfo with
            | Some r ->
                match r.original with
                | Some d -> d.Id, isSelected d.Id
                | _ -> null, false
            | _ -> null, false
        createObj [
            "onClick", unbox (fun (e, handleOriginal : (unit -> unit) option) ->
                match handleOriginal with
                | Some ha -> ha()
                | None -> ()
                if not (isNull id) then
                    toogle id
                )
            "style", createObj [
                "background", if isSelected then unbox "lightgreen" else null
            ]
        ]
        |> Some
    ))
    p.selectType <- Some ReactTable.SelectType.Checkbox

    ReactTable.selectTable p
    //str (sprintf "%d Demos" model.CurrentData.Length)

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let view (model : Model) (dispatch : Msg -> unit) =
    let inner =
        demosView model dispatch
    
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "CSGO Demo Manager" ] ] ]

          inner

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
