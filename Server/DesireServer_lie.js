// Setup basic express server
var express = require('express');
var app = express();
var http = require('http');
var io = require('socket.io')(server);
var port = process.env.PORT || 33333;

// MongoDB part
var MongoClient = require('mongodb').MongoClient;
var url = "mongodb://localhost/test";

http.createServer(function (req, res){
  res.writeHead(200, {'Content-Type':'text/html'});
  res.end('Hellow');
}).listen(port, function () {
  console.log('Server listening at port %d', port);
});

  // Routing
app.use(express.static(__dirname + '/public'));

io.on('connection', function(socket) {

);
