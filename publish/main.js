// Modules to control application life and create native browser window
const {app, BrowserWindow} = require('electron')
const path = require('path')
const process = require('process')
const child_process = require('child_process')

// Squirrel support https://github.com/mongodb-js/electron-squirrel-startup/blob/master/index.js
// https://github.com/electron/windows-installer
var debug = function() {
  console.log.apply(null, arguments);
};

var run = function(args, done) {
  var updateExe = path.resolve(path.dirname(process.execPath), '..', 'Update.exe');
  debug('Spawning `%s` with args `%s`', updateExe, args);
  child_process.spawn(updateExe, args, {
    detached: true
  }).on('close', done);
};

var check = function() {
  if (process.platform === 'win32') {
    var cmd = process.argv[1];
    debug('processing squirrel command `%s`', cmd);
    var target = path.basename(process.execPath);

    if (cmd === '--squirrel-install' || cmd === '--squirrel-updated') {
      // Optionally do things such as:
      // - Add your .exe to the PATH
      // - Write to the registry for things like file associations and
      //   explorer context menus
      run(['--createShortcut=' + target + ''], app.quit);
      return true;
    }
    if (cmd === '--squirrel-uninstall') {
      // Undo anything you did in the --squirrel-install and
      // --squirrel-updated handlers

      // Remove desktop and start menu shortcuts
      run(['--removeShortcut=' + target + ''], app.quit);
      return true;
    }
    if (cmd === '--squirrel-obsolete') {
      // This is called on the outgoing version of your app before
      // we update to the new version - it's the opposite of
      // --squirrel-updated

      app.quit();
      return true;
    }
  }
  return false;
};

if (check()) return;

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mainWindow

let rid = ""
let bin =  "CSGO-Demo-Backend"
let opsys = process.platform
if (opsys == "darwin") {
  rid = "osx.x64"
  bin = "CSGO-Demo-Backend.exe"
} else if (opsys == "win32") {
  rid = "win-x64"
} else if (opsys == "linux") {
  rid = "linux-x64"
}

if (rid === "") {
  throw "Cannot start CSGO-Demo-Backend"
}

global.sharedObject = {  
  server: 'http://localhost'
}

function findPort () {
  var net = require('net');

  return new Promise((resolve, reject) => {
    var srv = net.createServer(function(sock) {
      sock.end('Hello world\n');
    });
    srv.listen(0, function() {
      let port = srv.address().port;
      srv.close(function(err) {
        resolve(port);
      });
    });
  })
}
async function startServer () {
  let port = await findPort()
  global.sharedObject.server = `http://localhost:${port}`
  process.env["CSGO_BACKEND_SERVER"] = `http://localhost:${port}`

  let proc = child_process.spawn(`${__dirname}/Server/${rid}/${bin}`, [], {
    //detached: true,
    //stdio: [ 'ignore', 1, 2 ],
    env: { "SERVER_PORT": `${port}` }
  });
  //proc.unref();
  proc.stdout.on('data', function(data) {
      console.log("STDOUT: " +data.toString()); 
  });
  proc.stderr.on('data', function(data) {
      console.log("STDERR: " + data.toString()); 
  });
  proc.on("exit", (code, signal) => {
    console.log(`CSGO_BACKEND_SERVER exited: ${code} ${signal}`); 
  });
  //process.on("beforeExit", () => {
  //  proc.kill();
  //});

  // App close handler
  app.on('before-quit', async function() {
    proc.stdin.write('exit\n')
  });

  return proc;
}


let prc = startServer()

function createWindow () {
  // Create the browser window.
  mainWindow = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: true
    }
  })

  prc.then(() => {
    // and load the index.html of the app.
    mainWindow.loadFile('Client/index.html')
  })

  // Open the DevTools.
  // mainWindow.webContents.openDevTools()

  // Emitted when the window is closed.
  mainWindow.on('closed', function () {
    // Dereference the window object, usually you would store windows
    // in an array if your app supports multi windows, this is the time
    // when you should delete the corresponding element.
    mainWindow = null
  })
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', createWindow)

// Quit when all windows are closed.
app.on('window-all-closed', function () {
  // On macOS it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform !== 'darwin') app.quit()
})

app.on('activate', function () {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (mainWindow === null) createWindow()
})

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and require them here.
