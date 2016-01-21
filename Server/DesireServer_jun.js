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
    console.log('[new message] socket.position = %s', socket.position);
    console.log('\t[new message] socket.position ', data);

    // save data to db
 //    MongoClient.connect (url,
 //	function (err, db) {
 //	    if(err){
 //		console.log ('Error :', err);
 //	    }
 //	    else {
 //	    	db.collection("message").insert({
 //		    username: socket.username,
 //		    body: data,
 //		    position: socket.position
 //		    }, function (e, result) {
 //		    	console.log(e);
 //			console.log(result);
 //		    	db.close();
 //		    });
 //	    }
 //	}
 //    );

    // we tell the client to execute 'new message'
    socket.broadcast.to(socket.position).emit('new message', {
      username: socket.username,
      message: data
    });
  });

  // when the client emits 'add user', this listens and executes
  socket.on('add user', function (username, position) {
    if (addedUser) return;

    // we store the username in the socket session for this client
    socket.username = username;
    socket.position = position;
    socket.join(position);
    console.log ('[add user] socket.position = %s', socket.position);
    ++numUsers;
    addedUser = true;

    // Get recent 20 messages and return to logined user
//    MongoClient.connect(url,
//	    function (err, db) {
//		if(err) {
//		     console.log ('Error :', err);
//		}
//		else{	    
//		    db.collection("message").find({position: socket.position}).toArray(function (err, items) {
//		    if (err) {
//			console.log(err);
//		    }
//		    else {
//			var cnt = 0;
//		        for (var i in items) {
//		    	    if (cnt++ > items.length-21) {
//			    socket.emit('new message', {
//			        username: items[i].username,
//			        message: items[i].body
//		    	    });
//			    }
//		        }
//		    }
//		    db.close();
//		    });}
//	    });


    socket.emit('login', {
      numUsers: numUsers 
    });

    // echo globally (all clients) that a person has connected
    socket.broadcast.to(socket.position).emit('user joined', {
      username: socket.username,
      //numUsers: oMap.get(socket.position)
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
      });
    }
  });

  //when the user upload the image to the server
  socket.on('image', function (data){
    console.log('\timage ', data);
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
    console.log('\timagelist ', data.position);
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
            console.log('\tsave');
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
              console.log('\t\t send_list\n', send_list);
            }
          }
        }
        db.close();
      });
    });
  });
});
