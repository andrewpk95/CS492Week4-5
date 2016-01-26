package com.passion.hamfeed;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.PorterDuff;
import android.graphics.RectF;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.os.*;
import android.util.Log;
import android.view.Display;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.View;
import android.widget.ImageView;
import android.widget.Toast;

import com.google.gson.GsonBuilder;
import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;
import com.koushikdutta.ion.Response;
import com.squareup.picasso.Picasso;
import com.squareup.picasso.Target;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.lang.ref.WeakReference;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URISyntaxException;
import java.net.URL;
import java.net.URLConnection;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.Future;

import io.socket.client.IO;
import io.socket.client.Socket;

/**
 * Created by Junhong on 2016-01-25.
 */
public class ModifyActivity extends Activity{

    private final String TAG = "ModifyActivity";
    private ListItem modified_item;
    private String modify_user;
    private DrawView drawView = null;
    private static int dh = 0;
    private static int dw = 0;
    private final int SEND_INFO = 0;
    private MsgHandler msgHandler;
    private HttpConnect httpConnect;

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_modify, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        switch(id){
            case R.id.action_clear:
                drawView = null;
                drawView = new DrawView(this);
                drawView.setBackgroundColor(Color.WHITE);
                setBitmap();
                setContentView(drawView);
                break;
            case R.id.action_save:
                httpConnect = new HttpConnect(Constants.IMG_INFOR_URL);
                httpConnect.execute();
                break;
            case R.id.action_change_white:
                drawView.ChangeWhite();
                break;
            case R.id.action_change_red:
                drawView.ChangeRed();
                break;
            case R.id.action_change_blue:
                drawView.ChangeBlue();
                break;
        }

        /*if (id == R.id.action_clear) {
            Log.i(TAG, "action_clear is clicked");
            drawView.setBackgroundColor(Color.WHITE);
            drawView.getDrawingCache().eraseColor(Color.WHITE);
            drawView.destroyDrawingCache();
            drawView = null;
            drawView = new DrawView(this);
            setBitmap();

            return true;
        }else if(id == R.id.action_save){
            Log.i(TAG, "action_save is clicked");
            send information of picture before sending the picture
            UploadInfo uploadInfo = new UploadInfo(Constants.IMG_INFOR_URL, json_data);
            uploadInfo.run();
            SaveBitmapToFileCache(getBitmapFromView(drawView), "/temp/", "temp.png");
            return true;
        }else if(id == R.id.action_change_pen_color){

        }*/

        return super.onOptionsItemSelected(item);
    }

    /*public static Bitmap getBitmapFromView(View view) {
        //Define a bitmap with the same size as the view
//        Bitmap returnedBitmap = Bitmap.createBitmap(view.getWidth(), view.getHeight(), Bitmap.Config.ARGB_8888);
        Bitmap returnedBitmap = Bitmap.createBitmap(dw, dh, Bitmap.Config.ARGB_8888);
        //Bind a canvas to it
        Canvas canvas = new Canvas(returnedBitmap);
        //Get the view's background
        Drawable bgDrawable =view.getBackground();
        if (bgDrawable!=null)
            //has background drawable, then draw it on the canvas
            bgDrawable.draw(canvas);
        else
            //does not have background drawable, then draw white background on the canvas
            canvas.drawColor(Color.WHITE);
        // draw the view on the canvas
        view.draw(canvas);
        //return the bitmap
        return returnedBitmap;
    }*/

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        drawView = new DrawView(this);
        drawView.setBackgroundColor(Color.WHITE);

        msgHandler = new MsgHandler();

        getListItem();
        setBitmap();
        setContentView(drawView);
    }

    public void setBitmap(){
        Display currentDisplay = getWindowManager().getDefaultDisplay();
        dw = currentDisplay.getWidth();
        dh = currentDisplay.getHeight();
        drawView.setDrawingCacheEnabled(true);
        new LoadImgTask(drawView).execute(modified_item.getImg_url());
    }

    public void getListItem(){
        Intent intent = getIntent();
        if(intent == null){
            Log.i(TAG, "intent is null");
            finish();
        }else{
            modified_item = (ListItem) intent.getSerializableExtra("modify");
            modify_user = intent.getStringExtra("modifyuser");
            Log.i(TAG, modified_item.getImg_url() + " " + modify_user);
        }
    }

    public class DrawView extends View implements View.OnTouchListener {

        private Paint paint = new Paint();
        private Path path = new Path();

        public DrawView(Context context) {
            super(context);

            setFocusable(true);
            setFocusableInTouchMode(true);

            this.setOnTouchListener(this);

            paint = new Paint(Paint.ANTI_ALIAS_FLAG);
            paint.setStyle(Paint.Style.STROKE);
            paint.setStrokeWidth(2);
            paint.setColor(Color.BLACK);
        }

        @SuppressLint("DrawAllocation")
        @Override
        public void onDraw(Canvas canvas) {

            super.onDraw(canvas);
            canvas.drawPath(path, paint);
        }

        @Override
        public boolean onTouch(View arg0, MotionEvent event) {
            float eventX = event.getX();
            float eventY = event.getY();

            switch (event.getAction()) {
                case MotionEvent.ACTION_DOWN:
                    path.moveTo(eventX, eventY);
                    return true;
                case MotionEvent.ACTION_MOVE:
                case MotionEvent.ACTION_UP:
                    path.lineTo(eventX, eventY);
                    break;
                default:
                    return false;
            }

            // Schedules a repaint.
            invalidate();
            return true;
        }

        public void ChangeWhite(){
            paint.clearShadowLayer();
            paint.setColor(Color.WHITE);
        }

        public void ChangeRed(){
            paint.setColor(Color.RED);
        }

        public void ChangeBlue(){

            paint.setColor(Color.BLUE);

        }
    }

    public class LoadImgTask extends AsyncTask<String, Void, Bitmap> {
        private final WeakReference<DrawView> imageViewWeakReference;
        private String imageUrl;

        public LoadImgTask(DrawView drawView) {
            imageViewWeakReference = new WeakReference<DrawView>(drawView);
        }

        @Override
        protected Bitmap doInBackground(String... params) {
            imageUrl = params[0];
            return LoadImage(imageUrl);
        }

        @Override
        protected void onPreExecute(){
            super.onPreExecute();
        }

        @Override
        protected void onPostExecute(Bitmap bitmap) {
            super.onPostExecute(bitmap);
            if(imageViewWeakReference != null && bitmap != null){
                final DrawView drawView = imageViewWeakReference.get();
                if(drawView != null){
//                    getActivity().runOnUiThread(imgServerLoad);
                    drawView.setBackground(new BitmapDrawable(bitmap));
                }
            }
        }

        private Bitmap LoadImage(String URL){
            Bitmap compressed = null;
            InputStream in = null;

            int inWidth = 0;
            int inHeight = 0;

            int dstWidth = dw;
            int dstHeight = dh;

            try{
                in = OpenHttpConnection(URL);

                BitmapFactory.Options options = new BitmapFactory.Options();
                options.inJustDecodeBounds = true;
                Bitmap bitmap = BitmapFactory.decodeStream(in, null, options);
//                Log.i(TAG, "LoadImage URL " + URL);
//                Log.i(TAG, "format : " + URL.substring(URL.lastIndexOf(".") + 1));
//                Log.i(TAG, (in == null)? "true" : "false");

                // save width and height
                inWidth = options.outWidth;
                inHeight = options.outHeight;

                // decode full image pre-resized
                in = OpenHttpConnection(URL);
                options = new BitmapFactory.Options();
                // calc rought re-size (this is no exact resize)
                options.inSampleSize = Math.max(inWidth/dstWidth, inHeight/dstHeight);
                // decode full image
                Bitmap roughBitmap = BitmapFactory.decodeStream(in, null, options);

                // calc exact destination size
                Matrix m = new Matrix();
                RectF inRect = new RectF(0, 0, roughBitmap.getWidth(), roughBitmap.getHeight());
                RectF outRect = new RectF(0, 0, dstWidth, dstHeight);
                m.setRectToRect(inRect, outRect, Matrix.ScaleToFit.CENTER);
                float[] values = new float[9];
                m.getValues(values);

                // resize bitmap
                compressed = Bitmap.createScaledBitmap(roughBitmap, (int) (roughBitmap.getWidth() * values[0]), (int) (roughBitmap.getHeight() * values[4]), true);
                in.close();
            } catch (IOException e) {
                e.printStackTrace();
            }
            return compressed;
        }

        private InputStream OpenHttpConnection(String strURL) throws IOException{
            InputStream inputStream = null;
            URL url = new URL(strURL);
            URLConnection conn = url.openConnection();

            try{
                HttpURLConnection httpConn = (HttpURLConnection)conn;
                httpConn.setRequestMethod("GET");
                httpConn.connect();

                if(httpConn.getResponseCode() == HttpURLConnection.HTTP_OK){
                    inputStream = httpConn.getInputStream();
                }
            } catch(Exception e){
                e.printStackTrace();
            }

            return inputStream;
        }
    }

    // Bitmap to File
    /*public  void SaveBitmapToFileCache(Bitmap bitmap, String strFilePath,
                                       String filename) {

        File file = new File(strFilePath);

        // If no folders
        if (!file.exists()) {
            file.mkdirs();
            // Toast.makeText(this, "Success", Toast.LENGTH_SHORT).show();
        }

        File fileCacheItem = new File(strFilePath + filename);
        OutputStream out = null;

        try {
            fileCacheItem.createNewFile();
            out = new FileOutputStream(fileCacheItem);

            bitmap.compress(Bitmap.CompressFormat.JPEG, 100, out);
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            try {
                out.close();

                Future uploading = Ion.with(getApplicationContext())
                        .load(Constants.IMG_SERVER_URL)
                        .setMultipartFile("image", fileCacheItem)
                        .asString()
                        .withResponse()
                        .setCallback(new FutureCallback<Response<String>>() {
                            @Override
                            public void onCompleted(Exception e, Response<String> result) {
                                try {
                                    Log.i(TAG, "onCompleted " + result.getResult());
                                    JSONObject json = new JSONObject(result.getResult());
                                    Toast.makeText(getApplicationContext(), json.getString("response"), Toast.LENGTH_SHORT).show();
                                } catch (Exception e1) {
                                    e1.printStackTrace();
                                }
                            }
                        });
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }*/

    class UploadImg extends Thread{
        private File temp = null;
        private File sendFile = null;
        private String path = "/sdcard/temp/";

        public UploadImg(){
            temp = new File(path);
            FileOutputStream fileOutputStream = null;
            // If no folders
            if (!temp.exists()) {
                temp.mkdirs();
                // Toast.makeText(this, "Success", Toast.LENGTH_SHORT).show();
            }
            sendFile = new File(path + modified_item.getFileName());

            try {
                fileOutputStream = new FileOutputStream(sendFile);
                Bitmap b = drawView.getDrawingCache();
                b.compress(Bitmap.CompressFormat.JPEG, 95, fileOutputStream);
                fileOutputStream.close();
//                temp.delete();
            } catch (FileNotFoundException e) {
                e.printStackTrace();
            } catch (IOException e) {
                e.printStackTrace();
            }
        }

        @Override
        public void run(){
            super.run();
            Future uploading = Ion.with(getApplicationContext())
                    .load(Constants.IMG_SERVER_URL)
                    .setMultipartFile("image", sendFile)
                    .asString()
                    .withResponse()
                    .setCallback(new FutureCallback<Response<String>>() {
                        @Override
                        public void onCompleted(Exception e, Response<String> result) {
                            try {
                                Log.i(TAG, "onCompleted " + result.getResult());
                                JSONObject json = new JSONObject(result.getResult());
                                Toast.makeText(getApplicationContext(), json.getString("response"), Toast.LENGTH_SHORT).show();
                            } catch (Exception e1) {
                                e1.printStackTrace();
                            }
                        }
                    });

        }
    }

    class HttpConnect extends AsyncTask<Void, Void, Void>{
        private String json_data;
        private String url;
        private String currentTime;
        private Socket mSocket;
        {
            try {
                mSocket = IO.socket(Constants.CHAT_SERVER_URL);
            } catch (URISyntaxException e) {
                throw new RuntimeException(e);
            }
        }

        public HttpConnect(String pUrl){
            SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd_HHmmss");
            currentTime = sdf.format(new Date());

            //send json data using http indicating the file information
            Map<String, String> img_info_http = new HashMap<String, String>();
            img_info_http.put("username", modify_user);
            img_info_http.put("position", modified_item.getRoomnumber());
            img_info_http.put("time", currentTime);

//            Log.i(TAG, "HttpConnect " + currentTime);
            //it is possible to extract the image name on server, so do not including the file name
            json_data = new GsonBuilder().create().toJson(img_info_http, Map.class);

            url = pUrl;
            saveImage();
        }

        public void saveImage(){
            JSONObject img_info = new JSONObject();

            try {
                img_info.put("username", modify_user);
                img_info.put("position", modified_item.getRoomnumber());
                img_info.put("img_file_name", modified_item.getFileName());
                img_info.put("time", currentTime);
            } catch (JSONException e) {
                e.printStackTrace();
            }

            mSocket.emit("image", img_info);
        }

        public void makeRequest(String uri, String json) {
            try {
                URL url = new URL(uri);
                HttpURLConnection connection = (HttpURLConnection)url.openConnection();
                connection.setDoOutput(true);
                connection.setRequestMethod("POST");
                connection.setRequestProperty("Content-Type", "application/json");

                OutputStream os = connection.getOutputStream();
                os.write(json.getBytes());
                os.flush();

                if(connection.getResponseCode() != HttpURLConnection.HTTP_OK){
                    Log.i(TAG, "HttpURLConnection is not ok");
                    throw new RuntimeException("Failed : Http error code: " + connection.getResponseCode());
                }

                connection.disconnect();
            } catch (MalformedURLException e) {
                e.printStackTrace();
            } catch (IOException e) {
                e.printStackTrace();
            }

            msgHandler.sendEmptyMessage(SEND_INFO);
            return;
        }

        @Override
        protected Void doInBackground(Void... params) {
            makeRequest(url, json_data);
            return null;
        }
    }
    /*
    class UploadInfo extends Thread{
        private String uri;
        private String json;

        public UploadInfo(String pUri, String pJson){
            uri = pUri;
            json = pJson;
        }

        @Override
        public void run(){
            super.run();

            try {
                URL url = new URL(uri);
                HttpURLConnection connection = (HttpURLConnection)url.openConnection();
                connection.setDoOutput(true);
                connection.setRequestMethod("POST");
                connection.setRequestProperty("Content-Type", "application/json");

                OutputStream os = connection.getOutputStream();
                os.write(json.getBytes());
                os.flush();

                if(connection.getResponseCode() != HttpURLConnection.HTTP_OK){
                    Log.i(TAG, "HttpURLConnection is not ok");
                    throw new RuntimeException("Failed : Http error code: " + connection.getResponseCode());
                }

                connection.disconnect();
            } catch (MalformedURLException e) {
                e.printStackTrace();
            } catch (IOException e) {
                e.printStackTrace();
            }

            msgHandler.sendEmptyMessage(SEND_INFO);
            return ;
        }
    }*/

    class MsgHandler extends Handler {
        @Override
        public void handleMessage(android.os.Message msg){
            super.handleMessage(msg);

            switch(msg.what){
                case SEND_INFO:
                    UploadImg uploadImg = new UploadImg();
                    uploadImg.run();
                    break;
            }
        }
    }
}
