// Setup basic express server
var express = require('express');
var app = express();
var connect = require('connect');

//Configuration
app.use(express.static(__dirname + '/public'));
app.use(connect.cookieParser());
app.use(connect.logger('dev'));
app.use(connect.bodyParser());

app.use(connect.json());
app.use(connect.urlencoded());

// Routing
require('./routes.js')(app);

var server = require('http').createServer(app);
var io = require('socket.io')(server);
var port = process.env.PORT || 1111;
var port_img = process.env.PORT || 1112;

// MongoDB part
var MongoClient = require('mongodb').MongoClient;
var url = "mongodb://localhost/test";

server.listen(port, function () {
  console.log('Server listening at port %d', port);
});

// How many users in the same chatting room?
var numUsers = 0;

// Imgage Listview in order to store the current version of picture
var current_img = [];
var send_list = [];

// When the client upload the img file port
app.listen(port_img, function(){
  console.log('Server listeneing at port %d', port_img);
  console.log('Server numUsers ', numUsers);
});

io.on('connection', function(socket) {
  var addedUser = false;

  // when the client emits 'new message', this listens and executes
  socket.on('new message', function (data) {
    console.log('\t[new message] socket.position ', data);

    // save data to db
    MongoClient.connect (url, function (err, db) {
 	    if(err){
        console.log ('Error :', err);
 	    }
 	    else {
 	    	db.collection("message").insert({
          username: socket.username,
          body: data,
          position: socket.position
          }, function (e, result) {
            //console.log(e);
            //console.log(result);
            db.close();
        });
      }
    });

    // we tell the client to execute 'new message'
    socket.broadcast.to(socket.position).emit('new message', {
      username: socket.username,
      message: data
    });
  });

  // when the client emits 'add user', this listens and executes
  socket.on('add user', function (username, position) {
    //console.log('들어오나?', addedUser);
    if (addedUser) return;

    // we store the username in the socket session for this client
    socket.username = username;
    socket.position = position;
    socket.join(position);
    console.log ('[add user] socket.position = %s', socket.position);
    ++numUsers;
    addedUser = true;

    // Get recent 20 messages and return to logined user
    MongoClient.connect(url, function (err, db) {
      if(err) {
           console.log ('Error :', err);
      }
      else{	    
          db.collection("message").find({position: socket.position}).toArray(function (err, items) {
          if (err) {
            console.log(err);
          }
          else {
            var cnt = 0;
            for (var i in items) {
              if (cnt++ > items.length-51) {
                socket.emit('new message', {
                  username: items[i].username,
                  message: items[i].body
                });
              }
            }
          }
          db.close();
        });}
	    });

    socket.emit('login', {
      numUsers: numUsers 
    });

    // echo globally (all clients) that a person has connected
    socket.broadcast.to(socket.position).emit('user joined', {
      username: socket.username,
      numUsers: numUsers
    });
  });

  // when the client emits 'typing', we broadcast it to others
  socket.on('typing', function () {
    socket.broadcast.to(socket.position).emit('typing', {
      username: socket.username
    });
  });

  // when the client emits 'stop typing', we broadcast it to others
  socket.on('stop typing', function () {
    socket.broadcast.to(socket.position).emit('stop typing', {
      username: socket.username
    });
  });

  // when the user disconnects.. perform this
  socket.on('disconnect', function () {
    if (addedUser) {
      --numUsers;
      socket.leave(socket.position);

      // echo globally that this client has left
      socket.broadcast.to(socket.position).emit('user left', {
        username: socket.username,
        numUsers: numUsers
      });
    }
  });

  // when user leave the room
  socket.on('leave', function () {
    if (addedUser) {
      --numUsers;

      // echo globally that this client has left
      socket.broadcast.to(socket.position).emit('user left', {
        username: socket.username,
        numUsers: numUsers
      });
      addedUser = false;
    }
  });

  //when the user upload the image to the server
  socket.on('image', function (data){
    MongoClient.connect(url, function (err, db){
	    db.collection("HamImageInformation").insertOne(data);
      if(err){
        console.log('Error: ', err);
      }
      else {
        console.log('\tsave');
      }
      db.close();
    });
  });
  
  //In order to feed on the listview
  socket.on('imagelist', function (data){
    //query on the database and retrieve the results
    MongoClient.connect(url, function (err, db){
	    db.collection("HamImageInformation").find({"position":data.position}).toArray(function (err, items) {
        if(err){
          console.log('Error: ', err);
        }
        else {
          if(items.length > 0){
            current_img = [];
            send_list = [];
            //console.log('\tsave');
            var time_stamp = "0"; //just use for calculate the time order
            //the key should be username + img_file_name
            current_img[items[0].img_file_name] = items[0];

            items.forEach(function(entry){
              if(time_stamp.localeCompare(entry.time) < 0){
                time_stamp = entry.time;
                current_img[entry.img_file_name] = entry;
              }
              //console.log('\t\t\tconsole ', current_img[entry.img_file_name]);
            });

            //processing the img http address
            for(var key in current_img){
              var html_addr = "http://143.248.140.92/~user/img_temp_hamfeed/" + 
                              current_img[key].position + "_" + 
                              current_img[key].username + "_" +
                              current_img[key].time + "_" +
                              current_img[key].img_file_name;
              //json processing new type
              var data = {
                position: current_img[key].position,
                time: current_img[key].time,
                username: current_img[key].username,
                html_addr: html_addr
              };
              send_list.push(data);
              socket.emit('imageresponse', send_list);
              //console.log('\t\t send_list\n', send_list);
            }
          }
        }
        db.close();
      });
    });
  });

  //the version control
  socket.on('version', function (data){
    //query on the same version of image
    MongoClient.connect(url, function (err, db){
	    //db.collection("HamImageInformation").find({"position":data.position, "username":data.username, "img_file_name":data.filename}).toArray(function (err, items) {
	    db.collection("HamImageInformation").find({"position":data.position, "img_file_name":data.filename}).toArray(function (err, items) {
        if(err){
          console.log('Error: ', err);
        }
        else {
          if(items.length > 0){
            var version_list = [];
            
            items.forEach(function(entry){
              var html_addr = "http://143.248.140.92/~user/img_temp_hamfeed/" + 
                              entry.position + "_" + 
                              entry.username + "_" +
                              entry.time + "_" +
                              entry.img_file_name;
              var data = {
                position: entry.position,
                time: entry.time,
                username: entry.username,
                html_addr: html_addr 
              };
              version_list.push(data);
            });
            socket.emit('versionResponse', version_list);
          }
        }
        db.close();
      });
    });
  });

  //when the user leave the reply for the image
  socket.on('reply', function (data){
    MongoClient.connect(url, function (err, db){
	    db.collection("HamFeedImage").insertOne(data);
      if(err){
        console.log('Error: ', err);
      }
      else {
        console.log('\t reply is save');
      }
      db.close();
    });
  });

  //when there is a request to bring the replies bact to the client
  socket.on('replyRequest', function (data){
    //console.log('replyRequest is incoming', data);
    MongoClient.connect(url, function (err, db){
      db.collection("HamFeedImage").find(data).toArray(function (err, item){
        console.log('\t\t find\n', item);
        socket.emit('replyResponse', item);
        db.close();
      });
    });
  });

  //when a player request to join other player in the same chatting room 
  socket.on('play request', function (data){
    //console.log('play request is incoming', data);
    socket.broadcast.to(data.room).emit('play response', {
      username: data.username
    });
//    socket.broadcast.to(socket.position).emit('play response', {
//      username: socket.username,
//      message: data
//    });
  });
});
