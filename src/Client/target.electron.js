let server = require('electron').remote.getGlobal('sharedObject').server
global.targetApi = {
    server: server
}