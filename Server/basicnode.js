// basicServer.js
var http = require('http');
http.createServer(function (req, res) {
    res.writeHead(200, {'Content-Type': 'text/html'});
    res.end('Hello World hahahahahaha I did it!!!!!');
}).listen(1337, '143.248.140.92');
console.log('Server running at http://143.248.140.92:1337/');

var MongoClient = require('mongodb').MongoClient;
var url = "mongodb://localhost/test";

MongoClient.connect(url, function (err, db){
      db.collection("HamImageInformation").find({},  {position:1,time:1,username:1,img_file_name:1, _id:0}, function (err, data) {
      if(err){
        console.log('Error: ', err);
      }
      else {
       // console.log(data);
      }
      
    });
});


var Mongoose = require('mongoose');
Mongoose.connect('mongodb://localhost/test');


var db = Mongoose.connection;
db.on('error', console.error.bind(console, 'connection error:'));
db.once('open', function callback () {
  console.log('connection successful...');
});


//var db = require('mongojs').connect('test', ['HamImageInformation']); 

//db.collection("HamImageInformation").find({},  {position:1,time:1,username:1,img_file_name:1, _id:0}, function (error, data) { console.log(data); 
//});
