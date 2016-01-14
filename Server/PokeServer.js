// Setup basic express server
var express = require('express');
var app = express();
var server = require('http').createServer(app);
var io = require('socket.io')(server);
var port = process.env.PORT || 4000;

// MongoDB part
var MongoClient = require('mongodb').MongoClient;
var url = "mongodb://localhost/test";

server.listen(port, function () {
  console.log('Server listening at port %d', port);
});

// Routing
app.use(express.static(__dirname + '/public'));

io.on('connection', function(socket) {
  var addedUser = false;

  // when the client emits 'new message', this listens and executes
  socket.on('new message', function (data) {
    console.log('new message received: ', data);
    // we tell the client to execute 'new message'
    socket.emit('new message', data);
  }); 
  
  socket.on('ConnectionTest', function (data) {
    console.log('Connection Test received: ', data);
    
    socket.emit('ConnectionTest', {message: 'efgh'});
  });
  
  //Map Activity Connection
  socket.on('MapActivity', function (data){
    var wild_num = 4;
    var wild_array = {
      wild_num : wild_num, 
      wild: []
    };

    var wild_lat = 0;
    var wild_long = 0;

    for(i = 0; i < wild_num; i++){
      var random_lat = Math.random();
      var random_long = Math.random();

      // Scale since it's too big
      if(random_lat > 0.5){
        wild_lat = data.latitude + random_lat / 500;
      }else{
        wild_lat = data.latitude - random_lat / 500;
      }

      if(random_long > 0.5){
        wild_long = data.longitude + random_long / 500;
      }else{
        wild_long = data.longitude - random_long / 500;
      }
       
      var temp = {"latitude" : wild_lat,
                  "longitude" : wild_long}
      wild_array.wild.push({
        "latitude" : wild_lat,
        "longitude" : wild_long
      });
    }
    //current near data
    wild_array.wild.push({
      "latitude" : data.latitude + 0.0001,
      "longitude" : data.longitude + 0.0002});

    // we tell the client to execute 'new message'
    //socket.emit('new message', data);
    socket.emit('new message', wild_array);
    socket.IMEI = data.IMEI;
  });

  socket.on('fight', function (data){
    console.log('fight :', data);
    setTimeout(function(){
      socket.broadcast.to(socket.IMEI).emit('Response', data);
    }, 2000);
    socket.emit('Response', data);
    console.log('socket.IMEI : %s', socket.IMEI);
    console.log('Response is emitted and broadcasted :', data);
  });

  socket.on('Request', function (data) {
    console.log('Request Received: ', data);
    var jsonObj;
    try {
      jsonObj = data;
      //console.log(data);
      //console.log(data.toString().slice(0, -4));
      //jsonObj = JSON.parse(data.toString().slice(0, -4));
    }
    catch (e) {
      return console.error(e);
    }
    if (jsonObj.RequestType == "FindByIMEI") {
	//socket.emit('Response', {successful: true, message: 'haha'});
	console.log('Processing FindByIMEI...');
	MongoClient.connect(url, function (err, db) {
        if(err){
          console.log('Error: ', err);
        }
        else {
          db.collection("PokeDama").find({"IMEI":jsonObj.IMEI}).toArray(function (err, items) {
            if (err) {
              console.log('Error:', err);
            }
            else {
              if (items.length == 0) {
		socket.emit('Response', {
		  ResponseType: 'FindByIMEI',
		  successful: false,
		  message: null
		});
	      }
	      else if (items.length == 1) {
		socket.emit('Response', {
		  ResponseType: 'FindByIMEI',
		  successful: true,
		  message: items[0]
		});
	      }
	      else {
		socket.emit('Response', {
		  ResponseType: 'FindByIMEI',
		  successful: false,
		  message: 'error'
		});
	      }
	    }
	  });
	  console.log('Processing Complete');
	}
      });
    }
    else if (jsonObj.RequestType == "FindByLocation") {
	console.log('Processing FindByLocation...');
	MongoClient.connect(url, function (err, db) {
	if(err){
	  console.log('Error: ', err);
	}
	else {
	  var loc = jsonObj.location;
	  var range = jsonObj.range;
          db.collection("PokeDama").find({
	    "location.latitude": {$gt: loc.latitude - range}, 
	    "location.latitude": {$lt: loc.latitude + range},
	    "location.longtitude": {$gt: loc.longtitude - range},
	    "location.longtitude": {$lt: loc.longtitude + range}
	  }).toArray(function (err, items) {
            if (err) {
              console.log('Error:', err);
            }
            else if (item.length) {
              for (var i in items) {
		socket.emit('Response', {
		  ResponseType: 'FindByLocation',
		  successful: true,
		  message: i
		});
	      }
	    }
	    else {
	      console.log('No items found');
	      socket.emit('Response', {
		ResponseType: 'FindByLocation',
		successful: true,
		message: null
	      });
	    }
	    console.log('Processing Complete');
	  });
        }
      });
    }
    else if (jsonObj.RequestType == "Save") {
      console.log('Processing Save...');
      
      MongoClient.connect(url, function (err, db) {
        if(err){
	  console.log('Error: ', err);
        }
        else {
	  var pokedama = jsonObj.PokeDama;
	  
          db.collection("PokeDama").replaceOne(
	    {"IMEI":pokedama.IMEI},
	    pokedama
	  );
	  socket.emit('Response', {
	    ResponseType: 'Save',
	    successful: true,
	    message: pokedama
	  });
	  console.log('Processing Complete');
        }
      });
    }
    else if (jsonObj.RequestType == "Create") {
      console.log('Processing Create...');
      
      MongoClient.connect(url, function (err, db) {
	if (err) {
	  console.log('Error: ', err);
	}
	else {
	  var pokedama = jsonObj.PokeDama;
	  
	  db.collection("PokeDama").insertOne(pokedama);
	  socket.emit('Response', {
	    ResponseType: 'Create',
	    successful: true,
	    message: pokedama
	  });
	  console.log('Processing Complete');
	}
      });
    }
    else if(jsonObj.RequestType == "SaveScene"){
      //Save scene and uuid number on PokeScene DB
      
       MongoClient.connect(url, function (err, db) {
         if (err) {
           console.log('Error: ', err);
         }
         else {
           var scene = jsonObj.SaveScene;
           var num = 0;
           
            console.log('scene imei : ', scene.IMEI);

            db.collection("PokeScene").find({"IMEI":scene.IMEI}).toArray(function (err, items) {
              if (err) {
                console.log('Error:', err);
              }
              else {
                console.log('Processing Complete ', items);

                 if(items.length == 0){
                    console.log('         PokeScene insert ', scene);
                    db.collection("PokeScene").insertOne(scene);
                  } else{
                    console.log('         else PokeScene insert ', items[0].IMEI);
                    db.collection("PokeScene").findAndModify(
                      {IMEI: items[0].IMEI}, //QUERY
                      [['_id', 'asc']], //sort order
                      {$set: {scene: scene.scene}}, //replacement, replaces only the field "hi"
                      {}, //options
                      function(err, object){
                        if(err){
                          console.warn(err.message);
                        }else{
                          console.log('db update ', object);
                        }
                      });
                  }
              }
            });
           }
         });
      }
    else if(jsonObj.RequestType == "RetrieveScene"){
      //Save scene and uuid number on PokeScene DB
      console.log('Retrieve Scene : ', jsonObj.IMEI);

      MongoClient.connect(url, function (err, db) {
          if(err){
            console.log('Error: ', err);
          }
          else {

            db.collection("PokeScene").find({"IMEI":jsonObj.IMEI}).toArray(function (err, items) {
              if (err) {
                console.log('Error:', err);
              }
              else {
                console.log('   Processing Complete ', items[0]);
                console.log('   scene ', items[0].scene);
                socket.emit('Response', {
                  scene: items[0].scene 
                });
              }
            });

          }
        });
      }
  });
});
