module Backend

// resolve backend calls properly depending on the environment.
// webpack-dev-mode -> relative/absolute URLs (proxy is OK)
// electron -> use process.env["CSGO_BACKEND_SERVER"]
open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS

// require('electron').remote.getGlobal('sharedObject').server
[<Emit("global.targetApi.server")>]
let server: string = jsNative // importAll "electron"
let prefix =
    if isNull server then ""
    else server

        //let server : string = electron?remote?getGlobal("sharedObject")?server :> obj :?> string
        //if isNull server then ""
        //else server.TrimEnd('/')

let getUrl api =
    prefix + api