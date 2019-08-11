#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators

Target.initEnvironment ()

// AppName cannot contain '-', see https://github.com/electron/windows-installer/issues/187
let appName = "csgoDemoManager"
let winstoreName = "csgo.demo.manager"
let winstoreDisplayName = "CSGO Demo Manager"
let serverPath = Path.getFullName "./src/CSGO-Demo-Backend"
let clientPath = Path.getFullName "./src/Client"
let publishPath = Path.getFullName "./publish"
let clientDeployPath = Path.combine publishPath "Client"
let deployDir = Path.getFullName "./deploy"

let release = ReleaseNotes.load "RELEASE_NOTES.md"

let fourDigitVersion = 
    let v = System.Version.Parse release.AssemblyVersion
    let makeZero i = Math.Max(i, 0)
    sprintf "%d.%d.%d.%d" (makeZero v.Major) (makeZero v.Minor) (makeZero v.Build) (makeZero v.Revision) 

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ ->
        let errorMsg =
            tool + " was not found in path. " +
            "Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
        failwith errorMsg

let nodeTool = platformTool "node" "node.exe"
let yarnTool = platformTool "yarn" "yarn.cmd"

let runToolWithArgs cmd workingDir args =
    Command.RawCommand (cmd, args |> Arguments.OfArgs)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let runTool cmd args workingDir =
    let arguments = args |> String.split ' '
    runToolWithArgs cmd workingDir arguments

let runDotNet cmd workingDir =
    let result =
        DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

let openBrowser url =
    //https://github.com/dotnet/corefx/issues/10361
    Command.ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
    |> Proc.run
    |> ignore


Target.create "Clean" (fun _ ->
    [ deployDir
      clientDeployPath ]
    |> Shell.cleanDirs
)

Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    runTool nodeTool "--version" __SOURCE_DIRECTORY__
    printfn "Yarn version:"
    runTool yarnTool "--version" __SOURCE_DIRECTORY__
    runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
)

Target.create "Build" (fun _ ->
    runDotNet "build" serverPath
    Shell.regexReplaceInFileWithEncoding
        "let app = \".+\""
       ("let app = \"" + release.NugetVersion + "\"")
        System.Text.Encoding.UTF8
        (Path.combine clientPath "Version.fs")
    runTool yarnTool "webpack-cli -p" __SOURCE_DIRECTORY__
)

Target.create "Run" (fun _ ->
    let server = async {
        runDotNet "watch run" serverPath
    }
    let client = async {
        runTool yarnTool "webpack-dev-server" __SOURCE_DIRECTORY__
    }
    let browser = async {
        do! Async.Sleep 5000
        openBrowser "http://localhost:8080"
    }

    let vsCodeSession = Environment.hasEnvironVar "vsCodeSession"
    let safeClientOnly = Environment.hasEnvironVar "safeClientOnly"

    let tasks =
        [ if not safeClientOnly then yield server
          yield client
          if not vsCodeSession then yield browser ]

    tasks
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

let npmTool tool =
    let ext = if Environment.isWindows then ".cmd" else ""
    sprintf "./node_modules/.bin/%s%s" tool ext

Target.create "PublishServer" (fun _ ->
    let rids = [ "win-x64"; "osx-x64"; "linux-x64" ]
    for r in rids do
        let outDir = Path.getFullName (sprintf "./publish/Server/%s" r)
        let args = sprintf "publish -r %s -o \"%s\"" r outDir
        runDotNet args serverPath
)

Target.create "ElectronPackages" (fun _ ->
    [ "./publish"; appName; "--platform"; "win32,linux,darwin"; "--arch"; "x64"; "--out"; "./deploy/package"]    
    |> runToolWithArgs (npmTool "electron-packager") __SOURCE_DIRECTORY__

    // Cleanup & zip packages (as we don't need to bundle all binaries)
    let toCleanup =
          // electron folder -> rid to keep -> dir
        [ "darwin-x64", "osx-x64", sprintf "%s.app/Contents/Resources" appName
          "linux-x64", "linux-x64", "resources"
          "win32-x64", "win-x64", "resources" ]
    for electron, toKeep, subDir in toCleanup do
        let dirs = System.IO.Directory.EnumerateDirectories(sprintf "./deploy/package/%s-%s/%s/app/Server" appName electron subDir) |> Seq.toList
        for dir in dirs do
            let name = System.IO.Path.GetFileName dir
            if name <> toKeep then
                printfn "Deleting '%s' as it is not required" dir
                Shell.rm_rf dir

        let packageDir = sprintf "./deploy/package/%s-%s" appName electron
        let zipFile = sprintf "%s.zip" packageDir
        !! (sprintf "%s/**" packageDir)
        |> Zip.zip packageDir zipFile

        Trace.publish (ImportData.BuildArtifactWithName (sprintf "portable-%s" electron)) zipFile
)

Target.create "CreateWinInstaller" (fun _ ->
    // TODO: Set loadingGif, iconUrl
    let escapeSingleQuoteStr (s:string) =
        s.Replace("\\", "\\\\")
    let installerName = sprintf "setup-%s-%s" appName release.NugetVersion
    // https://github.com/electron/windows-installer
    let code =
        sprintf """(async () => {
  const electronInstaller = require('electron-winstaller');
  try {
    await electronInstaller.createWindowsInstaller({
      appDirectory: '%s',
      outputDirectory: '%s',
      authors: 'Matthias Dittrich',
      setupExe: '%s.exe',
      version: '%s',
      noMsi: true,
      exe: '%s.exe'
    });
    console.log('It worked!');
  } catch (e) {
    console.log(`Error creating installer: ${e.message}`);
    exit(1);
  }
})()
"""         (escapeSingleQuoteStr (Path.getFullName (sprintf "./deploy/package/%s-win32-x64" appName)))
            (escapeSingleQuoteStr (Path.getFullName ("./deploy/win-installer")))
            installerName
            release.NugetVersion
            (escapeSingleQuoteStr appName)
    [ "-e"; code ]
    |> runToolWithArgs nodeTool __SOURCE_DIRECTORY__

    Trace.publish (ImportData.BuildArtifactWithName "windows-installer") (sprintf "./deploy/win-installer/%s" installerName)
)

Target.create "CreateWinApp" (fun _ ->
    // make winstore appx https://github.com/felixrieseberg/electron-windows-store
    [ "--input-directory"; sprintf "./deploy/package/%s-win32-x64" appName
      "--output-directory"; "./deploy/win-store"
      "--package-version"; fourDigitVersion
      "--package-name"; winstoreName
      "--package-display-name"; winstoreDisplayName
      "-e"; sprintf "app/%s.exe" appName
      "--publisher"; "CN=matthid"]    
    |> runToolWithArgs (npmTool "electron-windows-store") __SOURCE_DIRECTORY__

    Trace.publish (ImportData.BuildArtifactWithName "windows-app") (sprintf "./deploy/win-store/%s.appx" winstoreName)

    // TODO: https://www.christianengvall.se/dmg-installer-electron-app/
)


Target.create "Full" ignore

open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "Build"
    ==> "PublishServer"
    ==> "ElectronPackages"

"ElectronPackages"
    ==> "CreateWinInstaller"
"ElectronPackages"
    ==> "CreateWinApp"

"CreateWinInstaller"
    ==> "Full"
"CreateWinApp"
    ==> "Full"


"Clean"
    ==> "InstallClient"
    ==> "Run"

Target.runOrDefaultWithArguments "Full"
