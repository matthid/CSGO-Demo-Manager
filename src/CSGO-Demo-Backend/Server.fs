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

let webApp =
    route "/api/init" >=>
        fun next ctx ->
            task {
                let cache = ctx.GetService<ICacheService>()
                let! demos = cache.GetDemoListAsync() |> Async.AwaitTask
                let counter = { Demos = demos |> Seq.map ConvertToShared.ofDemo |> List.ofSeq }
                return! json counter next ctx
            }

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
