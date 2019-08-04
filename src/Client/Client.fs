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

type CurrentSort =
  { Id : string
    Desc : bool
  } with
    interface ReactTable.SortingRule with
        member x.id with get () = x.Id and set v = x?id <- v
        member x.desc with get () = x.Desc and set v = x?desc <- v

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = {
    LoadingData : bool
    Pages : int
    SelectedDemos : string list
    SortingRule : CurrentSort
    CurrentStart: int
    CurrentPageSize: int
    CurrentData: Demo list }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| LoadingData of start:int * pageSize:int * sort:CurrentSort
| NewData of DemoData
| StartDownloadMM
| SelectionChanged of string list

let fetchDemos (startItem:int) (maxItems:int) (sort:CurrentSort) =
    let url = sprintf "/api/demos?startItem=%d&maxItems=%d&sortBy=%s&desc=%b" startItem maxItems sort.Id sort.Desc
    Fetch.fetchAs<DemoData> (Backend.getUrl url)
    //|> Promise.map (fun d -> d)

    //Cmd.OfPromise.perform fetch () (fun d -> NewData(startItem, d))
// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let defaultRule = { Id = "Date"; Desc = true }
    let initialModel = { CurrentData = []; LoadingData = true; Pages = 1; SelectedDemos = []; CurrentStart = 0; CurrentPageSize = 20; SortingRule = defaultRule }
    let loadCountCmd = Cmd.OfPromise.result (fetchDemos 0 20 defaultRule |> Promise.map NewData)
    initialModel, loadCountCmd

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | LoadingData (start, pageSize, sort) ->
        let startOk = start = currentModel.CurrentStart
        let pageSizeOk =  pageSize = currentModel.CurrentPageSize
        let sortOk = sort.Id = currentModel.SortingRule.Id && sort.Desc = currentModel.SortingRule.Desc
        if currentModel.LoadingData || (startOk && pageSizeOk && sortOk) then
            printfn "ignore LoadingData() -> (loading: %b, startOk: %b, pageSizeOk: %b, sortOk: %b)" currentModel.LoadingData startOk pageSizeOk sortOk
            currentModel, Cmd.none
        else
            let nextModel =
                { currentModel with
                    LoadingData = true
                    CurrentPageSize = pageSize
                    CurrentStart = start
                    SortingRule = sort }
        
            nextModel, Cmd.OfPromise.result (fetchDemos start pageSize sort |> Promise.map NewData)
    | SelectionChanged sel ->
        let nextModel = { currentModel with SelectedDemos = sel }
        nextModel, Cmd.none
    | NewData (data) ->
        let nextModel =
            { currentModel with
                CurrentData = data.Demos; LoadingData = false;
                Pages = data.Pages; SelectedDemos = [] }
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
    
let sourceView (cell:ReactTable.CellInfo<Demo, string>) =
    span [] [str (string cell.value)]
let dateView (cell:ReactTable.CellInfo<Demo, System.DateTime>) =
    span [] [str (string cell.value)]
let formatSecs (secs:FloatNum) =
    let hours = System.Math.Floor(secs / 3600.)
    let minutes = System.Math.Floor((secs - hours * 3600.) / 60.)
    let seconds = System.Math.Ceiling((secs - hours * 3600.) - minutes * 60.)
    let hours = if hours < 10. then "0" + string hours else string hours
    let minutes = if minutes < 10. then "0" + string minutes else string minutes
    let seconds = if seconds < 10. then "0" + string seconds else string seconds
    hours+":"+minutes+":"+seconds;
    
let durationView (cell:ReactTable.CellInfo<Demo, FloatNum>) =
    match cell.value with
    | Some secs -> span [] [str (formatSecs secs)]
    | _ -> span [] [str (string cell.value)]
    
let columns =
    let asColum (c:ReactTable.Column<Demo, 't>) =
        c :?> ReactTable.Column<Demo>
    let c0 =
        let c = createEmpty<ReactTable.Column<Demo, string>>
        c.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "Source")
        c.id <- Some "Source"
        c.maxWidth <- Some 100.
        c.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.SourceName))
        c.Cell <- Some (ReactTable.TableCellRenderer.ofCase1 (fun info -> sourceView info))
        asColum c
    let c1 =
        let c = createEmpty<ReactTable.Column<Demo, System.DateTime>>
        c.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "Date")
        c.id <- Some "Date"
        c.maxWidth <- Some 100.
        c.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.Date))
        c.Cell <- Some (ReactTable.TableCellRenderer.ofCase1 (fun info -> dateView info))
        asColum c
    let c2 =
        let c = createEmpty<ReactTable.Column<Demo, string>>
        c.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "Name")
        c.id <- Some "Name"
        c.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.Name))
        asColum c
    let c3 =
        let c = createEmpty<ReactTable.Column<Demo, string>>
        c.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "Comment")
        c.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.Comment))
        c.id <- Some "Comment"
        c.Cell <- Some (ReactTable.TableCellRenderer.ofCase1 (fun info -> span [] [str (string info.value)]))
        asColum c
    let c4 =
        let c = createEmpty<ReactTable.Column<Demo, string>>
        c.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "Map")
        c.id <- Some "Map"
        c.maxWidth <- Some 100.
        c.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.MapName))
        asColum c
    let c5 =
        let c = createEmpty<ReactTable.Column<Demo, FloatNum>>
        c.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "Duration")
        c.id <- Some "Duration"
        c.maxWidth <- Some 80.
        c.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.Duration))
        c.Cell <- Some (ReactTable.TableCellRenderer.ofCase1 (fun info -> durationView info))
        asColum c
    let c6 =
        let c = createEmpty<ReactTable.Column<Demo, string>>
        c.Header <- Some (ReactTable.TableCellRenderer.ofReactElement <| str "HostName")
        c.id <- Some "HostName"
        c.accessor <- Some (ReactTable.Accessor.ofFunc (fun d -> Some d.Hostname))
        asColum c
    [|c0; c1 ; c2; c3; c4; c5; c6 |]

let demosView (model:Model) dispatch =
    let p = createEmpty<ReactTable.SelectTableProps<Demo, Demo>>
    p.data <- model.CurrentData |> ResizeArray
    p.columns <- Some columns
    p.loading <- model.LoadingData
    p.pages <- Some (float model.Pages)
    p.page <- Some (float model.CurrentStart / float model.CurrentPageSize)
    p.pageSize <- Some (float model.CurrentPageSize)
    p.multiSort <- false
    p.sorted <- ResizeArray [{ Desc = model.SortingRule.Desc; Id = model.SortingRule.Id } :> ReactTable.SortingRule]
    p.manual <- true

    let startLoadingData newStart newPageSize (sortRule:CurrentSort) =
        dispatch(LoadingData(newStart, newPageSize, sortRule))
        
    p.onSortedChange <- ReactTable.SortedChangeFunction(fun newSort col additive ->
        let rule = newSort.[0]
        // not clear why this hack is required but it won't allow us to change the sorting rule on a single column otherwise.
        let rule = { Id = rule.id; Desc = if model.SortingRule.Id = rule.id then not model.SortingRule.Desc else rule.desc }
        startLoadingData model.CurrentStart model.CurrentPageSize rule
    )
    p.onFetchData <- (fun state instance ->
        let newStart = int state.pageSize * int state.page
        let newPageSize = int state.pageSize
        let rule = state.sorted.[0]
        let rule = { Id = rule.id; Desc = rule.desc }
        startLoadingData newStart newPageSize rule
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
    
    p.toggleAll <- Some (fun _ ->
        let isUnselected =
            model.CurrentData
            |> List.exists (fun d -> not (isSelected d.Id))
        let newSelected =
            if isUnselected then
                model.CurrentData
                |> List.map (fun d -> d.Id)
            else
                []
        
        dispatch (SelectionChanged newSelected)
    )

    p.selectAll <-
        model.CurrentData
        |> List.exists (fun d -> not (isSelected d.Id))
        |> not
        |> Some

    p.getTrProps <- U2.Case1 (ReactTable.ComponentPropsGetterR(fun state rowInfo column instance ->
        let id, isSelected =
            match rowInfo with
            | Some r ->
                match r.original with
                | Some d -> d.Id, isSelected d.Id
                | _ -> null, false
            | _ -> null, false
        createObj [
            "onClick", unbox (System.Action<Browser.Types.MouseEvent, (unit -> unit) option>(fun e handleOriginal ->
                match handleOriginal with
                | Some ha -> ha()
                | None -> ()
                if not (isNull id) && (e.altKey || e.ctrlKey) then
                    toogle id
                else
                    dispatch (SelectionChanged [ id ])
                ))
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

let buttons model dispatch =

    Container.container [] [
        Columns.columns [] [
            Column.column [] [ button "Analyse" (fun _ -> ()) ]
            Column.column [] [ button "Download MM Demos" (fun _ -> dispatch StartDownloadMM) ]
            Column.column [] [ button "Watch" (fun _ -> ()) ]
        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    let inner =
        demosView model dispatch
    
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "CSGO Demo Manager" ] ] ]

          buttons model dispatch
          demosView model dispatch

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
