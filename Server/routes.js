var fs = require('fs');

module.exports = function(app){
  app.get('/', function(res, res){
    res.end("Node-File-Upload");
  });

  app.post('/upload', function(req, res){
    console.log(req.files.image.originalFilename);
    console.log(req.files.image.path);
    console.log("let it go 11");
      fs.readFile(req.files.image.path, function(err, data){
        var dirName = "/home/user/public_html/img_temp_hamfeed";
        var newPath = dirName + "/" + req.files.image.originalFilename;
        fs.writeFile(newPath, data, function(err){
          console.log("let it go 16");
          if(err){
            res.json({'response' : "Error"});
          } else {
            res.json({'response' : "Saved"});
          }
          console.log("let it go 22");
        });
      });
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
