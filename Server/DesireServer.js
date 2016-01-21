// Setup basic express server
var express = require('express');
var app = express();
var server = require('http').createServer(app);
var io = require('socket.io')(server);
var port = process.env.PORT || 2222;

// MongoDB part
var MongoClient = require('mongodb').MongoClient;
var url = "mongodb://localhost/test";

server.listen(port, function () {
  console.log('Server listening at port %d', port);
});

  // Routing
app.use(express.static(__dirname + '/public'));

var hostIP;

io.on('connection', function(socket) {
  var addedUser = false;
  
  socket.on('ConnectionTest', function (data) {
    console.log('Connection Test Received: ', data);
    socket.emit('ConnectionTest', {message: 'efgh'});
  });
  
  socket.on('JoinRoom', function (data) {
    console.log('Connect Request Received: ', data);
    
    if (hostIP == null) {
      hostIP = socket.request.connection.remoteAddress.substring(7);
      console.log('New host: ' + hostIP);
      socket.emit('JoinRoom', {
        'isHost': true,
        'HostIP': hostIP
      });
    }
    else {
      var clientIP = socket.request.connection.remoteAddress.substring(7);
      console.log('New client: ' + clientIP);
      socket.emit('JoinRoom', {
        'isHost': false,
        'HostIP': hostIP,
        'ClientIP': clientIP
      });
    }
  });
  
  socket.on('disconnect', function (data) {
    var IP = socket.request.connection.remoteAddress.substring(7);
    if (IP == hostIP) {
      console.log('Host (' + IP + ') Disconnected...');
      hostIP = null;
    }
    else {
      console.log('Client (' + IP + ') Disconnected...');
    }
  });
});
