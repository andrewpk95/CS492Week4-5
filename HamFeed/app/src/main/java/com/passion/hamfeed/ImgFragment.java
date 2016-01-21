package com.passion.hamfeed;

import android.app.Activity;

import android.content.ActivityNotFoundException;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Bundle;
import android.provider.MediaStore;
import android.support.v4.app.Fragment;
import android.support.v4.content.CursorLoader;
import android.support.v7.widget.RecyclerView;
import android.text.format.Time;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.Toast;

import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;
import com.koushikdutta.ion.Response;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.lang.ref.WeakReference;
import java.net.HttpURLConnection;
import java.net.URISyntaxException;
import java.net.URL;
import java.net.URLConnection;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;
import java.util.List;
import java.util.SimpleTimeZone;
import java.util.concurrent.Future;

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
    private ListItem[] listItems;
    private String[] img_html_urls;
    private LoadImgTask feedlv;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setHasOptionsMenu(true);
        Ion.getDefault(getActivity()).configure().setLogging("ion-sample", Log.DEBUG);

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
                    //send the file to server
                    File img_file = new File(img_path);

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

                    //send the server to indicate the user upload the img file
                    JSONObject img_info = new JSONObject();

                    try {
                        img_info.put("username", mUsername);
                        img_info.put("position", mPosition);
                        img_info.put("img_file_name", img_path.substring(img_path.lastIndexOf("/") + 1));
                        SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd_HHmmss");
                        String currentTime = sdf.format(new Date());
                        img_info.put("time", currentTime);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }

                    mSocket.emit(imgUpload, img_info);
                }
            }
        });

        show_img = (ImageView)view.findViewById(R.id.show_img);

        lv = (ListView)view.findViewById(R.id.img_list);
//        ArrayList<ListItem> srcList = new ArrayList<ListItem>(Arrays.asList(listItems));
//        lv.setAdapter(new CustomListAdapter(getContext(), srcList));
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

                Log.i(TAG, "mUsername : " + mUsername + " mPosition : " + mPosition);
            }
            return;
        }

        img_path = getPathFromURI(data.getData());
        show_img.setImageURI(data.getData());
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

//                    Log.i(TAG, i + " th " + listItems[i].getAuthor() + " " + listItems[i].getImg_url() +
//                            " " + listItems[i].getTimestamp());

                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }

            //call handler to invoke the listview
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
            //query on server with chatting room number
            JSONObject data = new JSONObject();

            try {
                data.put("position", mPosition);
            } catch (JSONException e) {
                e.printStackTrace();
            }

            mSocket.emit("imagelist", data);

            //and retrieve the JSON array result showing on the listview
        }

        @Override
        protected void onPostExecute(Bitmap bitmap) {
            super.onPostExecute(bitmap);
            if(imageViewWeakReference != null && bitmap != null){
                final ImageView imageView = imageViewWeakReference.get();
                if(imageView != null){
                    imageView.setImageBitmap(bitmap);
                }
            }
        }

        private Bitmap LoadImage(String URL){
            Bitmap bitmap = null;
            InputStream in = null;

            try{
                in = OpenHttpConnection(URL);
                bitmap = BitmapFactory.decodeStream(in);
                in.close();
            } catch (IOException e) {
                e.printStackTrace();
            }
            return bitmap;
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

            if(icon != null){
                new LoadImgTask(icon).execute(listData.get(position).getImg_url());
            }
            return rowView;
        }
    }
}
