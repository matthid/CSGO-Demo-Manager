module NotificationsView 

open Elmish
open Elmish.React
open Shared
open Thoth.Fetch
open Fetch

type TaskState =
    | Running
    | Cancelled
    | Completed

type CurrentTask = {
    ServerTask : BackgroundTask
    State : TaskState
} 

let fromServerTask (s) =
    { ServerTask = s; State = Running }

[<AutoOpen>]
module CurrentTaskExt =
    type CurrentTask with
        member x.Id = x.ServerTask.Id
        member x.Name = x.ServerTask.Name
        member x.Progress = x.ServerTask.Progress
        member x.Messages = x.ServerTask.Messages
        member x.SetServerTask tsk =
            { x with ServerTask = tsk }
        member x.SetMessages msgs =
            x.SetServerTask { x.ServerTask with Messages = msgs }
        member x.AddMessage msg =
            x.SetMessages (x.ServerTask.Messages @ [ msg ])
        member x.SetProgress progress =
             x.SetServerTask { x.ServerTask with Progress = progress }

type Model = {
    Tasks : CurrentTask list
    LastNotification : Notification }

type Msg =
    | Notification of Notification
    | ReceivedTasks of res:BackgroundTasks
    | CancelTask of res:BackgroundTaskId
    | CancelTaskOk of res:string
    | RemoveTask of res:BackgroundTaskId
    | UpdateTasks
    //| ProgressUpdate of BackgroundTaskId * double
    //| MessageUpdate of BackgroundTaskId * string
    //| TaskStarted of BackgroundTask
    //| TaskCompleted of BackgroundTaskId

let downloadTasks () =
    let url = sprintf "/api/tasks"
    Fetch.fetchAs<BackgroundTasks> (Backend.getUrl url)
let cancelTask (tskId:BackgroundTaskId) =
    let url = sprintf "/api/tasks/%s" tskId
    Fetch.fetchAs<string> (Backend.getUrl url, [ Method HttpMethod.DELETE ])

let init () : Model * Cmd<Msg> =
    let model = { LastNotification = Notification.Hint "Initialize Application ... "; Tasks = [] }
    model, Cmd.none

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | Notification n ->
        // handle task started/closed
        let newModel = { currentModel with LastNotification = n }
        let newModel =
            match n with
            | Notification.TaskStarted t ->
                { newModel with Tasks = fromServerTask t :: newModel.Tasks }
            | Notification.TaskCompleted tid ->
                { newModel with Tasks = newModel.Tasks |> List.map (fun t -> if t.Id = tid then { t with State = Completed } else t) }
            | Notification.TaskMessageChanged (tid, msg) ->
                { newModel with Tasks = newModel.Tasks |> List.map (fun t -> if t.Id = tid then t.AddMessage msg else t) }
            | Notification.TaskProgressChanged (tid, progress) ->
                { newModel with Tasks = newModel.Tasks |> List.map (fun t -> if t.Id = tid then t.SetProgress progress else t) }
            | _ -> newModel

        newModel, Cmd.none
        
    | UpdateTasks ->
        currentModel, Cmd.OfPromise.result (downloadTasks () |> Promise.map ReceivedTasks)
    | RemoveTask tskId ->
        { currentModel with Tasks = currentModel.Tasks |> List.filter (fun t -> t.Id <> tskId) }, Cmd.none
    | CancelTask tskId ->
        { currentModel with Tasks = currentModel.Tasks |> List.map (fun t -> if t.Id = tskId then { t with State = Cancelled } else t) },
            Cmd.OfPromise.result (cancelTask tskId |> Promise.map CancelTaskOk)
    | CancelTaskOk s ->
        currentModel, Cmd.none
    | ReceivedTasks tsks ->
        //  mark missing stuff finished but don't remove them
        let knownTasks, newTasks = tsks.Tasks |> List.partition (fun tsk -> currentModel.Tasks |> Seq.exists (fun t -> t.Id = tsk.Id))
        let updatedTasks = 
            currentModel.Tasks |> List.map (fun tsk ->
                match knownTasks |> List.tryFind (fun t -> t.Id = tsk.Id) with
                | Some serverTask ->
                    tsk.SetServerTask(serverTask)
                | None -> // task is finished
                    { tsk with State = Completed })
        { currentModel with Tasks = updatedTasks @ (newTasks |> List.map fromServerTask) }, Cmd.none

open Fable.React
open Fable.React.Props
open Fetch.Types
open Thoth.Fetch
open Fulma
open Thoth.Json
open Fable.Core
open Fable.Core.JsInterop
    
let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let showNotification (n:Notification) =
    let str =
        match n with
        | Notification.Hint h -> str h
        | Notification.DemosFound d -> str (System.String.Join(",", d))
        | Notification.Error err -> str err
        | _ -> str ""
    div [  Style [ Color "gray" ] ] [ str ]


let view (model:Model) (dispatch:Dispatch<Msg>) =
    let stateButtons (tsk:CurrentTask) =
        match tsk.State with
        | Running -> [ button "Cancel" (fun _ -> dispatch (CancelTask tsk.Id)) ; button "Remove" (fun _ -> dispatch (RemoveTask tsk.Id)) ]
        | Cancelled -> [ button "Remove" (fun _ -> dispatch (RemoveTask tsk.Id)) ]
        | Completed -> [ button "Remove" (fun _ -> dispatch (RemoveTask tsk.Id)) ]
    let stateCell (tsk:CurrentTask) =
        match tsk.State with
        | Running -> str "Running"
        | Cancelled -> str "Cancelled"
        | Completed -> str "Completed"
    let rows =
        model.Tasks
        |> List.map (fun tsk ->
            let lastMsg = tsk.Messages |> Seq.tryLast |> Option.defaultValue ""
            tr [] [
                td [] [ str tsk.Id ]
                td [] [ str lastMsg ]
                td [] [ str tsk.Name ]
                td [] [ str (sprintf "%f %%" tsk.Progress) ]
                td [] [stateCell tsk]
                td [] (stateButtons tsk)
            ] )
    div [] [
        showNotification model.LastNotification
        Table.table [] [
            tbody [] rows
        ]
    ]