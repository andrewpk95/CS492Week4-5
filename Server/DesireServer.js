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
  
  socket.on('ResultSend', function (data) {
    console.log('Saving result: ', data);
    
    MongoClient.connect(url, function (err, db) {
      if (err) {
        console.log('Error: ', err);
      }
      else {
        db.collection("BlastResult").insertOne(data);
        console.log('Saved!');
      }
    });
  });
  
  socket.on('ResultRequest', function (data) {
    console.log('Sending result to: ', data);
    
    MongoClient.connect(url, function (err, db) {
      if (err) {
        console.log('Error: ', err);
      }
      else {
        db.collection("BlastResult").find({}).toArray(function (err, items) {
          if (err) {
            console.log('Error finding: ', err);
          }
          else {
              //console.log('\t\titems\n', items[0].winner);
            if (items.length != 1) {
              socket.emit('ResultResponse', {
                successful: false,
                data: null
              });
            }
            else {
              socket.emit('ResultResponse', {
                successful: true,
                data: items[0].winner
              });
            }
          }
        });
        db.collection("BlastResult").remove({});
        console.log('Result Sent.');
      }
    });
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
