var fs = require('fs');

//picture meta data
var position;
var timestamp;
var username;
var imgname;

module.exports = function(app){
  app.get('/', function(res, res){
    res.end("Node-File-Upload");
  });

  app.post('/upload', function(req, res){
    console.log(req.files.image.path);  // /tmp? 
    imgname = req.files.image.originalFilename;
    console.log('\t', position, '\t', timestamp, '\t', username, '\t', imgname);
      fs.readFile(req.files.image.path, function(err, data){
        var dirName = "/home/user/public_html/img_temp_hamfeed";
        var newPath = dirName + "/" + position + "_" + 
                      username + "_" + timestamp + "_" +
                      req.files.image.originalFilename;
        fs.writeFile(newPath, data, function(err){
          if(err){
            res.json({'response' : "Error"});
          } else {
            res.json({'response' : "Saved"});
          }
        });
      });
  });
  
  app.post('/info', function(req, res){
    console.log('/info request is incoming ', req.body);
    position = req.body.position;
    timestamp = req.body.time;
    username = req.body.username;
  });

  app.get('/uploads/:file', function(req, res){
    file = req.params.file;
    var dirName = "/home/user/img_temp_hamfeed";
    var img = fs.readFileSync(dirName + file);
    console.log("let it go 30");
    res.writeHead(200, {'Content-Type' : 'image/jpg'});
    res.end(img, 'binary');
  });
};
