open System
open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection

open FSharp.Control.Tasks.V2
open Giraffe
open Shared
open Services.Interfaces
open Services.Concrete.Excel
open Services.Concrete
open System.Collections.Generic

type IMyDemoService =
    abstract member Cache : Async<IReadOnlyCollection<Core.Models.Demo>> with get
type MyDemoService(cache : ICacheService) =
    let demos = cache.GetDemoListAsync()
    interface IMyDemoService with
        member x.Cache
            with get () =
                async {
                    let! de = demos |> Async.AwaitTask
                    return de :> IReadOnlyCollection<_>
                }

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath =
    let t1 = Path.GetFullPath "../Client/deploy"
    let t2 = Path.GetFullPath "./Client"
    let t3 = Path.GetFullPath "./Client/deploy"
    let all = [ t1; t2; t3 ]
    all
    |> Seq.tryFind (fun p ->
        File.Exists (Path.Combine(p, "index.html")))
    |> Option.defaultWith (fun () ->
        failwithf "Could not find client directory, tried %A" all)    
let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

module Seq =
    let tryTake (n : int) (s : _ seq) =
        seq {
            use e = s.GetEnumerator ()
            let mutable i = 0
            while e.MoveNext () && i < n do
                i <- i + 1
                yield e.Current
        }
    let trySkip (n : int) (s : _ seq) =
        seq {
            use e = s.GetEnumerator ()
            let mutable i = 0
            let mutable dataAvailable = e.MoveNext ()
            while dataAvailable && i < n do
                dataAvailable <- e.MoveNext ()
                i <- i + 1
            if dataAvailable then
                yield e.Current
                while e.MoveNext () do
                    yield e.Current
        }

let webApp =
    choose [
        route "/api/downloadMM" >=>
            fun next ctx ->
                task {
                    let r = { Status = "Ok" }
                    return! json r next ctx
                }
        route "/api/demos" >=>
            fun next ctx ->
                task {
                    let startItem =
                        let t = ctx.Request.Query.["startItem"]
                        if t.Count > 0 then Int32.Parse t.[0] else 0
                    let maxItems =
                        let t = ctx.Request.Query.["maxItems"]
                        if t.Count > 0 then Int32.Parse t.[0] else 50
                    let sortBy =
                        let t = ctx.Request.Query.["sortBy"]
                        if t.Count > 0 then t.[0] else "Date"
                    let desc =
                        let t = ctx.Request.Query.["desc"]
                        if t.Count > 0 then bool.Parse t.[0] else true

                    let cache = ctx.GetService<IMyDemoService>()
                    let! demos = cache.Cache
                    let sortFunc proj s = s |> (if desc then Seq.sortByDescending proj else Seq.sortBy proj)
                    let sortedDemos =
                        match sortBy with
                        | "Date" -> demos |> sortFunc (fun d -> d.Date)
                        | "Name" ->  demos |> sortFunc (fun d -> d.Name)
                        | "Hostname" -> demos |> sortFunc (fun d -> d.Hostname)
                        | "Duration" -> demos |> sortFunc (fun d -> d.Duration)
                        | _ -> failwithf "unknown sort column '%s'" sortBy
                    printfn "Have %d demos, skipping %d and taking %d" demos.Count startItem maxItems
                    let demoData =
                        { Demos = sortedDemos |> Seq.trySkip startItem |> Seq.tryTake maxItems |> Seq.map ConvertToShared.ofDemo |> List.ofSeq
                          Pages = demos.Count / maxItems + (if demos.Count % maxItems = 0 then 0 else 1) }
                    return! json demoData next ctx
                }
    ]

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles()
       .UseStaticFiles()
       .UseGiraffe webApp

let configureServices (services : IServiceCollection) =

    // Create run time view services and models
    services.AddSingleton<IDemosService, DemosService>() |> ignore<IServiceCollection>
    services.AddSingleton<ISteamService, SteamService>() |> ignore<IServiceCollection>
    services.AddSingleton<ICacheService, CacheService>() |> ignore<IServiceCollection>
    services.AddSingleton<ExcelService, ExcelService>() |> ignore<IServiceCollection>
    services.AddSingleton<IFlashbangService, FlashbangService>() |> ignore<IServiceCollection>
    services.AddSingleton<IKillService, KillService>() |> ignore<IServiceCollection>
    services.AddSingleton<IRoundService, RoundService>() |> ignore<IServiceCollection>
    services.AddSingleton<IPlayerService, PlayerService>() |> ignore<IServiceCollection>
    services.AddSingleton<IDamageService, DamageService>() |> ignore<IServiceCollection>
    services.AddSingleton<IStuffService, StuffService>()|> ignore<IServiceCollection>
    services.AddSingleton<IAccountStatsService, AccountStatsService>() |> ignore<IServiceCollection>
    services.AddSingleton<IMyDemoService, MyDemoService>() |> ignore<IServiceCollection>
    //services.AddSingleton<IMapService, MapService>();
    //services.AddSingleton<IDialogService, DialogService>();

    services.AddGiraffe() |> ignore
    services.AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer()) |> ignore

let host =
    WebHost
        .CreateDefaultBuilder()
        .UseWebRoot(publicPath)
        .UseContentRoot(publicPath)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
        .Build()

host.StartAsync().GetAwaiter().GetResult()
// Start to build demo cache
host.Services.GetService<IMyDemoService>() |> ignore<IMyDemoService>

printfn "Started server, write 'exit<Enter>' to stop the server"
let mutable hasExited = false
while not hasExited do
    let currentCommand = System.Console.ReadLine()
    if currentCommand = "exit" then
        hasExited <- true
    else    
        printfn "Unknown command '%s'" currentCommand

host.StopAsync().GetAwaiter().GetResult()
printfn "Proper Backend Shutdown finished"
