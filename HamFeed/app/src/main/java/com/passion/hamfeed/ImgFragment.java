package com.passion.hamfeed;

import android.app.Activity;

import android.content.ActivityNotFoundException;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Matrix;
import android.graphics.RectF;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.provider.MediaStore;
import android.support.v4.app.Fragment;
import android.support.v4.content.CursorLoader;
import android.support.v7.widget.RecyclerView;
import android.text.format.Time;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import com.google.gson.GsonBuilder;
import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;
import com.koushikdutta.ion.Response;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.File;
import java.io.FileInputStream;
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
import java.util.List;
import java.util.Map;
import java.util.SimpleTimeZone;
import java.util.concurrent.Future;
import java.util.logging.LogRecord;

import io.socket.client.IO;
import io.socket.client.Socket;
import io.socket.emitter.Emitter;

import static com.passion.hamfeed.MessageAdapter.*;

/**
 * Created by Junhong on 2016-01-17.
 */
public class ImgFragment extends Fragment {

    private String TAG = "ImgFragment";    //to debug
    private Button send_img_sever, upload_img;
    private String img_path;
    private ImageView show_img;
    private String mUsername;
    private String mPosition;
    private String currentTime;
    private Socket mSocket;
    {
        try {
            mSocket = IO.socket(Constants.CHAT_SERVER_URL);
        } catch (URISyntaxException e) {
            throw new RuntimeException(e);
        }
    }

    private ListView lv;

    private final String imgUpload = "image";
    private final String imgFeed = "imageresponse";

    private final int FEED_LISTVIEW = 0;

    private ListItem[] listItems;
    private String[] img_html_urls;
    private LoadImgTask feedlv;
    private MsgHandler mHandler;
    private Context mContext;
    private HttpConnect httpConnect;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setHasOptionsMenu(true);
        Ion.getDefault(getActivity()).configure().setLogging("ion-sample", Log.DEBUG);
        mContext = getContext();
        mSocket.on(imgFeed, onImage);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_img, container, false);
    }

    @Override
    public void onViewCreated(View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        mHandler = new MsgHandler();

        send_img_sever = (Button)view.findViewById(R.id.send_img_to_server);
        send_img_sever.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                //asynctask patch is required due to heavy work
                Intent gallery_intent = new Intent(Intent.ACTION_GET_CONTENT);
                gallery_intent.setType("image/jpg");   //Do we need to keep jpeg format?
                try{
                    startActivityForResult(gallery_intent, Constants.SELECT_IMG);
                } catch (ActivityNotFoundException e){
                    e.printStackTrace();
                }
            }
        });

        upload_img = (Button)view.findViewById(R.id.upload_img);
        upload_img.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                //asynctask is required due to heavy work on main thread
                if (img_path == null) {

                } else {
                    //set the time stamp
                    SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd_HHmmss");
                    currentTime = sdf.format(new Date());

                    //send the server to indicate the user upload the img file
                    JSONObject img_info = new JSONObject();

                    try {
                        img_info.put("username", mUsername);
                        img_info.put("position", mPosition);
                        img_info.put("img_file_name", img_path.substring(img_path.lastIndexOf("/") + 1));
                        img_info.put("time", currentTime);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }

                    mSocket.emit(imgUpload, img_info);

                    //send information of picture before sending the picture
                    httpConnect = new HttpConnect(Constants.IMG_INFOR_URL);
                    httpConnect.execute();

                    //send the file to server
                    File img_file = new File(img_path);

                    //Compress the image
                    Future uploading = Ion.with(getContext())
                            .load(Constants.IMG_SERVER_URL)
                            .setMultipartFile("image", img_file)
                            .asString()
                            .withResponse()
                            .setCallback(new FutureCallback<Response<String>>() {
                                @Override
                                public void onCompleted(Exception e, Response<String> result) {
                                    try {
                                        Log.i(TAG, "onCompleted " + result.getResult());
                                        JSONObject json = new JSONObject(result.getResult());
                                        Toast.makeText(getContext(), json.getString("response"), Toast.LENGTH_SHORT).show();
                                    } catch (Exception e1) {
                                        e1.printStackTrace();
                                    }
                                }
                            });

                    sendRequest();
                    img_file = null;
                }
            }
        });

        show_img = (ImageView)view.findViewById(R.id.show_img);

        lv = (ListView)view.findViewById(R.id.img_list);
        lv.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                //Log.i(TAG, "onItemClick Listener");
                ListItem item = (ListItem)parent.getItemAtPosition(position);
//                Log.i(TAG, "user " + item.getAuthor() + " Img_url " + item.getImg_url() + " filename " + item.getFileName());
                Intent slide = new Intent(mContext, ImgSlideActivity.class);
                Bundle info_item = new Bundle();
                info_item.putString("position", item.getRoomnumber());
                info_item.putString("username", item.getAuthor());
                info_item.putString("filename", item.getFileName());
                info_item.putString("timestamp", item.getTimestamp());
                info_item.putString("replyuser", mUsername);
                slide.putExtras(info_item);
                startActivity(slide);
            }
        });
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if(data == null){
            return;
        }

        if (Activity.RESULT_OK != resultCode) {
            getActivity().finish();
            return;
        }

        if(Constants.SELECT_IMG != requestCode){
            if(mUsername == null & mPosition == null) {
                mUsername = data.getStringExtra("username");
                mPosition = data.getStringExtra("position");

                JSONObject json_data = new JSONObject();

                try {
                    json_data.put("position", mPosition);
                } catch (JSONException e) {
                    e.printStackTrace();
                }

                mSocket.emit("imagelist", json_data);

//                Log.i(TAG, "mUsername : " + mUsername + " mPosition : " + mPosition);
            }
            return;
        }

        img_path = getPathFromURI(data.getData());
//        show_img.setImageURI(data.getData());
        //change into bitmap and compress it
        getActivity().runOnUiThread(imgLocalLoad);
    }

    public void sendRequest(){
        if(mUsername != null & mPosition != null) {
            JSONObject json_data = new JSONObject();

            try {
                json_data.put("position", mPosition);
            } catch (JSONException e) {
                e.printStackTrace();
            }

            mSocket.emit("imagelist", json_data);
        }
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        mSocket.disconnect();
    }

    private String getPathFromURI(Uri contentUri) {
        String[] proj = { MediaStore.Images.Media.DATA };
        CursorLoader loader = new CursorLoader(getContext(), contentUri, proj, null, null, null);
        Cursor cursor = loader.loadInBackground();
        int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
        cursor.moveToFirst();
        return cursor.getString(column_index);
    }

    private Emitter.Listener onImage = new Emitter.Listener(){

        @Override
        public void call(Object... args) {
            JSONArray data_arr = (JSONArray)args[0];

            listItems = new ListItem[data_arr.length()];
            img_html_urls = new String[data_arr.length()];

            for(int i = 0; i < data_arr.length(); i++){
                try {
                    listItems[i] = new ListItem();
                    img_html_urls[i] = new String();

                    JSONObject json = data_arr.getJSONObject(i);
                    listItems[i].setAuthor(json.getString("username"));
                    listItems[i].setImg_url(json.getString("html_addr"));
                    img_html_urls[i] = json.getString("html_addr");
                    listItems[i].setTimestamp(json.getString("time"));
                    listItems[i].setRoomnumber(json.getString("position"));

//                    Log.i(TAG, i + " th " + listItems[i].getAuthor() + " " + listItems[i].getImg_url() +
//                            " " + listItems[i].getTimestamp());

                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }

            //call handler to invoke the listview
            mHandler.sendEmptyMessage(FEED_LISTVIEW);
        }
    };

    public class LoadImgTask extends AsyncTask<String, Void, Bitmap>{
        private final WeakReference<ImageView> imageViewWeakReference;
        private String imageUrl;

        public LoadImgTask(ImageView imageView) {
            imageViewWeakReference = new WeakReference<ImageView>(imageView);
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
                final ImageView imageView = imageViewWeakReference.get();
                if(imageView != null){
//                    getActivity().runOnUiThread(imgServerLoad);
                    imageView.setImageBitmap(bitmap);
                }
            }
        }

        private Bitmap LoadImage(String URL){
            Bitmap compressed = null;
            InputStream in = null;

            Log.i(TAG, "LoadImage URL " + URL);
            Log.i(TAG, "format : " + URL.substring(URL.lastIndexOf(".") + 1));
            int inWidth = 0;
            int inHeight = 0;

            int dstWidth = 150;
            int dstHeight = 150;

            try{
                in = OpenHttpConnection(URL);

                BitmapFactory.Options options = new BitmapFactory.Options();
                options.inJustDecodeBounds = true;
                Bitmap bitmap = BitmapFactory.decodeStream(in, null, options);
                in.close();
                in = null;

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

    public class CustomListAdapter extends BaseAdapter{
        private ArrayList<ListItem> listData;
        private LayoutInflater layoutInflater;

        public CustomListAdapter(Context context, ArrayList<ListItem> listData){
            this.listData = listData;
            layoutInflater = LayoutInflater.from(context);
        }

        @Override
        public int getCount() {
            return listData.size();
        }

        @Override
        public Object getItem(int position) {
            return listData.get(position);
        }

        @Override
        public long getItemId(int position) {
            return position;
        }

        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
            View rowView = layoutInflater.inflate(R.layout.item_list, parent, false);
            ImageView icon = (ImageView)rowView.findViewById(R.id.icon);
            TextView user = (TextView)rowView.findViewById(R.id.user);
            TextView time = (TextView)rowView.findViewById(R.id.timestamp);

            if(icon != null){
                new LoadImgTask(icon).execute(listData.get(position).getImg_url());
            }

            user.setText(listData.get(position).getAuthor());
            time.setText(listData.get(position).getTimestamp());

            return rowView;
        }
    }

    class MsgHandler extends Handler {
        @Override
        public void handleMessage(Message msg){
            super.handleMessage(msg);

            switch(msg.what){
                case FEED_LISTVIEW:
//                    Log.i(TAG, listItems.toString());

                    ArrayList<ListItem> srcList = new ArrayList<ListItem>(Arrays.asList(listItems));
                    lv.setAdapter(new CustomListAdapter(mContext, srcList));

                    break;

            }
        }
    }

    class HttpConnect extends AsyncTask<Void, Void, Void>{
        private String json_data;
        private String url;

        public HttpConnect(String pUrl){
            //send json data using http indicating the file information
            Map<String, String> img_info_http = new HashMap<String, String>();
            img_info_http.put("username", mUsername);
            img_info_http.put("position", mPosition);
            img_info_http.put("time", currentTime);
//            Log.i(TAG, "HttpConnect " + currentTime);
            //it is possible to extract the image name on server, so do not including the file name
            json_data = new GsonBuilder().create().toJson(img_info_http, Map.class);

            url = pUrl;
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

            return ;
        }

        @Override
        protected Void doInBackground(Void... params) {
            makeRequest(url, json_data);
            return null;
        }
    }

    class CompressionThread extends Thread{
        private String imgPath;
        private Bitmap original;
        private Bitmap compressed;

        public CompressionThread(Bitmap pOrigin, String path){
            original = pOrigin;
            imgPath = path;
        }

        @Override
        public void run(){
            super.run();
            //compress the resolution
            /*ByteArrayOutputStream baos = new ByteArrayOutputStream();
            if(imgPath.substring(imgPath.lastIndexOf(".") + 1).matches("PNG") ||
                    imgPath.substring(imgPath.lastIndexOf(".") + 1).matches("png")){
                original.compress(Bitmap.CompressFormat.PNG, 10, baos);
            } else if(imgPath.substring(imgPath.lastIndexOf(".") + 1).matches("jpeg") ||
                    imgPath.substring(imgPath.lastIndexOf(".") + 1).matches("jpg") ||
                    imgPath.substring(imgPath.lastIndexOf(".") + 1).matches("JPEG") ||
                    imgPath.substring(imgPath.lastIndexOf(".") + 1).matches("JPG")){
                original.compress(Bitmap.CompressFormat.JPEG, 10, baos);
            }
            compressed = BitmapFactory.decodeStream(new ByteArrayInputStream(baos.toByteArray()));*/

            try{
                int inWidth = 0;
                int inHeight = 0;

                int dstWidth = 400;
                int dstHeight = 400;

                InputStream in = null;

                in = new FileInputStream(imgPath);

                // decode image size (decode metadata only, not the whole image)
                BitmapFactory.Options options = new BitmapFactory.Options();
                options.inJustDecodeBounds = true;
                BitmapFactory.decodeStream(in, null, options);
                in.close();
                in = null;

                // save width and height
                inWidth = options.outWidth;
                inHeight = options.outHeight;

                // decode full image pre-resized
                in = new FileInputStream(imgPath);
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

            }
            catch (IOException e)
            {
                Log.e("Image", e.getMessage(), e);
            }
        }

        public Bitmap getCompressed(){
            return compressed;
        }
    }

    private Runnable imgLocalLoad = new Runnable() {
        @Override
        public void run() {
            Bitmap bitmap = BitmapFactory.decodeFile(img_path);
            CompressionThread ct = new CompressionThread(bitmap, img_path);
            ct.run();
            Bitmap decoded = ct.getCompressed();
            show_img.setImageBitmap(decoded);
        }
    };
}
