#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System
open System.IO

open Fake.Api
open Fake.BuildServer
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Tools

Target.initEnvironment ()

// AppName cannot contain '-', see https://github.com/electron/windows-installer/issues/187
let githubRepositoryName = "CSGO-Demos-Manager"
let defaultGitHubUser = "matthid"
let appName = "csgoDemoManager"
let winstoreName = "csgo.demo.manager"
let winstoreDisplayName = "CSGO Demo Manager"
let serverPath = Path.getFullName "./src/CSGO-Demo-Backend"
let clientPath = Path.getFullName "./src/Client"
let publishPath = Path.getFullName "./publish"
let clientDeployPath = Path.combine publishPath "Client"
let deployDir = Path.getFullName "./deploy"

let release = ReleaseNotes.load "RELEASE_NOTES.md"

BuildServer.install [
    AppVeyor.Installer
    TeamFoundation.Installer
]

let version =
    let segToString = function
        | PreReleaseSegment.AlphaNumeric n -> n
        | PreReleaseSegment.Numeric n -> string n
    let source, buildMeta =
        match BuildServer.buildServer with
        // For others see FAKE
        | BuildServer.TeamFoundation ->
            let sourceBranch = TeamFoundation.Environment.BuildSourceBranch
            let isPr = sourceBranch.StartsWith "refs/pull/"
            let firstSegment =
                if isPr then
                    let splits = sourceBranch.Split '/'
                    let prNum = bigint (int splits.[2])
                    [ PreReleaseSegment.AlphaNumeric "pr"; PreReleaseSegment.Numeric prNum ]
                else
                    []
            let buildId = bigint (int TeamFoundation.Environment.BuildId)
            [ yield! firstSegment
              yield PreReleaseSegment.Numeric buildId
            ], sprintf "vsts.%s" TeamFoundation.Environment.BuildSourceVersion
        | _ ->
            [ PreReleaseSegment.AlphaNumeric "local" ], ""

    let semVer = SemVer.parse release.NugetVersion
    let prerelease =
        match semVer.PreRelease with
        | None -> None
        | Some p ->
            let toAdd = System.String.Join(".", source |> Seq.map segToString)
            let toAdd = if System.String.IsNullOrEmpty toAdd then toAdd else "." + toAdd
            Some ({p with
                        Values = p.Values @ source
                        Origin = p.Origin + toAdd })
    let fromRepository =
        match prerelease with
        | Some _ -> { semVer with PreRelease = prerelease; Original = None; BuildMetaData = buildMeta }
        | None -> semVer

    match Environment.environVarOrNone "FAKE_VERSION" with
    | Some ver -> SemVer.parse ver
    | None -> fromRepository

let simpleVersion = version.AsString

let nugetVersion =
    if System.String.IsNullOrEmpty version.BuildMetaData
    then version.AsString
    else sprintf "%s+%s" version.AsString version.BuildMetaData

Trace.setBuildNumber nugetVersion

let fourDigitVersion = 
    Version(int version.Major, int version.Minor, int version.Patch, 0).ToString()

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

let vault =
    match Vault.fromFakeEnvironmentOrNone() with
    | Some v -> v
    | None -> TeamFoundation.variables

let getVarOrDefault name def =
    match vault.TryGet name with
    | Some v -> v
    | None -> Environment.environVarOrDefault name def
let mutable secrets = []
let releaseSecret replacement name =
    let secret =
        lazy
            let env = 
                match getVarOrDefault name "default_unset" with
                | "default_unset" -> failwithf "variable '%s' is not set" name
                | s -> s
            if BuildServer.buildServer <> BuildServer.TeamFoundation then
                // on TFS/VSTS the build will take care of this.
                TraceSecrets.register replacement env
            env
    secrets <- secret :: secrets
    secret
let certPass = releaseSecret "<cert-pw>" "certificate_password"
let certFile = getVarOrDefault "certificate_file" ""
let createCertificate = getVarOrDefault "create_certificate" "true" = "true"
let githubtoken = releaseSecret "<githubtoken>" "github_token"
let github_release_user = getVarOrDefault "github_release_user" defaultGitHubUser
let artifactsDir = getVarOrDefault "artifactsdirectory" ""
let fromArtifacts = not <| String.isNullOrEmpty artifactsDir

let nodeTool = platformTool "node" "node.exe"
let yarnTool = platformTool "yarn" "yarn.cmd"


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

Target.create "BuildServer" (fun _ ->
    runDotNet "build" serverPath
)

Target.create "BuildClient" (fun _ ->
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

Target.create "PublishServer" (fun _ ->
    let rids = [ "win-x64"; "osx-x64"; "linux-x64" ]
    for r in rids do
        let outDir = Path.getFullName (sprintf "./publish/Server/%s" r)
        let args = sprintf "publish -r %s -o \"%s\"" r outDir
        runDotNet args serverPath
)

Target.create "ElectronPackages" (fun _ ->
    [ "run"; "electron-packager"; "./publish"; appName; "--platform"; "win32,linux,darwin"; "--arch"; "x64"; "--out"; "./deploy/package"]    
    |> runToolWithArgs yarnTool __SOURCE_DIRECTORY__

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

        Trace.publish (ImportData.BuildArtifactWithName "portable-files") zipFile
)

let installerName = sprintf "setup-%s-%s" appName simpleVersion
Target.create "CreateWinInstaller" (fun _ ->
    // TODO: Set loadingGif, iconUrl
    let escapeSingleQuoteStr (s:string) =
        s.Replace("\\", "\\\\")
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

    Trace.publish (ImportData.BuildArtifactWithName "windows-installer") (sprintf "./deploy/win-installer/%s.exe" installerName)
)

let openSSLPath =
    let OpenSSLPath = @"[ProgramFiles]\Git\usr\bin\;[ProgramFilesX86]\Git\usr\bin\;"
    if Environment.isUnix then
        "openssl"
    else
        let ev = Environment.environVar "OPENSSL"
        if not (String.isNullOrEmpty ev) then ev else Process.findPath "OpenSSLPath" OpenSSLPath "openssl.exe"

Target.create "CreateWinApp" (fun _ ->
    let sortArch s =
        match s with
        | "x64" -> 10
        | "x86" -> 9
        | _ -> 0
    let pf86 = Environment.GetEnvironmentVariable("ProgramW6432")
    let programFiles =
        [ if not (System.String.IsNullOrEmpty pf86) then
            yield pf86
          yield Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
          yield Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) ]
        |> List.map IO.Path.GetFullPath
        |> List.distinct

    let windowsKit =
        programFiles
        |> Seq.map (fun p -> System.IO.Path.Combine(p, "Windows Kits", "10", "bin"))
        |> Seq.filter (System.IO.Directory.Exists)
        |> Seq.collect (fun p -> !! (p + "/*/*/makeappx.exe") |> Seq.toList)
        |> Seq.map (System.IO.Path.GetDirectoryName)
        |> Seq.groupBy (fun archDir ->
            let arch = IO.Path.GetFileName archDir
            let versionDir = System.IO.Path.GetDirectoryName archDir
            let version = IO.Path.GetFileName versionDir
            arch, version)
        |> Seq.map (fun ((arch, version), dir) ->
            match dir |> Seq.toList with
            | [ dir ] -> (arch, version), dir 
            | [] -> failwithf "(impossible) no dir for arch %s and version %s" arch version
            | dirs -> failwithf "(impossible) multiple dirs for arch %s and version %s: %A" arch version dirs)
        |> Seq.choose (fun ((arch, version), dir) ->
            match Version.TryParse version with
            | true, v -> Some((arch, v), dir)
            | _ -> 
                eprintfn "Ignoring '%s' folder in windows kits as '%s' could not be parsed to a version" dir version 
                None)
        |> Seq.sortByDescending (fun ((arch, version), dir) -> version, sortArch arch)
        |> Seq.tryHead
        
    let windowsKit =
        match windowsKit with
        | Some (_, k) -> k
        | None -> failwithf "could not find a Windows 10 SDK, please install it (required for creating a windows app)"
    // Create cert if required
    let cert, certPw =
        if createCertificate then
            let dir = IO.Path.GetFullPath "./publish/certs/gen"
            Directory.ensure dir
            // https://blog.didierstevens.com/2008/12/30/howto-make-your-own-cert-with-openssl/
            if not (File.Exists "./publish/certs/gen/codesign.key") then // create CA key 
                runTool openSSLPath "genrsa -out codesign.key 4096" dir
            if not (File.Exists "./publish/certs/gen/codesign.crt") then // self sign CA
                // https://www.ibm.com/support/knowledgecenter/en/SSB23S_1.1.0.13/gtps7/cfgcert.html
                runTool openSSLPath "req -new -x509 -config ../codesign.cfg -key codesign.key -out codesign.crt" dir
            if not (File.Exists "./publish/certs/gen/codesign.pfx") then
                // https://stackoverflow.com/questions/22327160/enter-export-password-to-generate-a-p12-certificate/22328260#22328260
                // https://www.ssl.com/how-to/create-a-pfx-p12-certificate-file-using-openssl/
                runTool openSSLPath "pkcs12 -export -out codesign.pfx -inkey codesign.key -in codesign.crt -password pass:test123" dir
            printfn "Using locally created certificate"
            "./publish/certs/gen/codesign.pfx", "test123"
        else 
            printfn "Using certificate provided via parameters"
            certFile, certPass.Value       
    
    // make winstore appx https://github.com/felixrieseberg/electron-windows-store
    [ "run"; "electron-windows-store"
      "--input-directory"; sprintf "./deploy/package/%s-win32-x64" appName
      "--output-directory"; "./deploy/win-store"
      "--package-version"; fourDigitVersion
      "--package-name"; winstoreName
      "--package-display-name"; winstoreDisplayName
      "-e"; sprintf "app/%s.exe" appName
      "--publisher"; "CN=matthid.de"
      "--windows-kit"; windowsKit
      "--dev-cert"; Path.getFullName cert
      "--cert-pass"; certPw ]    
    |> runToolWithArgs yarnTool __SOURCE_DIRECTORY__

    Trace.publish (ImportData.BuildArtifactWithName "windows-app") (sprintf "./deploy/win-store/%s.appx" winstoreName)

    // TODO: https://www.christianengvall.se/dmg-installer-electron-app/
)


Target.create "CI" ignore


Target.create "PrepareArtifacts" (fun _ ->
    if not fromArtifacts then
        Trace.trace "empty artifactsDir."
    else
        Trace.trace "ensure artifacts."
        let files =
            !! (artifactsDir + sprintf "/portable-files/%s-*.zip" appName)
            |> Seq.toList
        Trace.tracefn "files: %A" files
        files
        |> Shell.copy ("./deploy/package")

        [ (artifactsDir + sprintf "/windows-installer/%s.exe" installerName) ]
        |> Shell.copy ("./deploy/win-installer")

        [ artifactsDir + sprintf "/windows-app/%s.appx" winstoreName ]
        |> Shell.copy ("./deploy/win-store")
)

Target.create "Release_Github" (fun _ ->

    let token = githubtoken.Value
    let auth = sprintf "%s:x-oauth-basic@" token
    let url = sprintf "https://%sgithub.com/%s/%s.git" auth github_release_user githubRepositoryName

    let gitDirectory = "" // getVarOrDefault "git_directory" ""
    if not BuildServer.isLocalBuild then
        Git.CommandHelper.directRunGitCommandAndFail gitDirectory "config user.email matthi.d@gmail.com"
        Git.CommandHelper.directRunGitCommandAndFail gitDirectory "config user.name \"Matthias Dittrich\""
    //if gitDirectory <> "" && BuildServer.buildServer = BuildServer.TeamFoundation then
    //    Trace.trace "Prepare git directory"
    //    Git.Branches.checkout gitDirectory false TeamFoundation.Environment.BuildSourceVersion
    //else
    //    Git.Staging.stageAll gitDirectory
    //    Git.Commit.exec gitDirectory (sprintf "Bump version to %s" simpleVersion)
    //    let branch = Git.Information.getBranchName gitDirectory
    //    Git.Branches.pushBranch gitDirectory "origin" branch

    Git.Branches.tag gitDirectory simpleVersion
    Git.Branches.pushTag gitDirectory url simpleVersion

    let files =
        !! "./deploy/package/*.zip" |> Seq.toList
        |> List.append [ sprintf "./deploy/win-installer/%s.exe" installerName; sprintf "./deploy/win-store/%s.appx" winstoreName ]

    GitHub.createClientWithToken token
    |> GitHub.draftNewRelease github_release_user githubRepositoryName simpleVersion (release.SemVer.PreRelease <> None) release.Notes
    |> GitHub.uploadFiles files
    |> GitHub.publishDraft
    |> Async.RunSynchronously
)

open Fake.Core.TargetOperators

// Regular Build
"Clean"
    ==> "InstallClient"
    ==> "BuildClient"
    ==> "ElectronPackages"

"Clean"
    ==> "BuildServer"
    ==> "PublishServer"
    ==> "ElectronPackages"

// CI Build
"ElectronPackages"
    ==> "CreateWinInstaller"
"ElectronPackages"
    ==> "CreateWinApp"

"CreateWinInstaller"
    ==> "CI"
"CreateWinApp"
    ==> "CI"

// Release - Deployment State
(if fromArtifacts then "PrepareArtifacts" else "CI")
    ==> "Release_Github"

"Clean"
    ==> "InstallClient"
    ==> "Run"

Target.runOrDefaultWithArguments "ElectronPackages"
