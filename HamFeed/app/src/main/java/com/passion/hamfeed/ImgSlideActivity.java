package com.passion.hamfeed;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.support.v4.app.FragmentActivity;
import android.support.v4.view.ViewPager;
import android.util.Log;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.net.URISyntaxException;
import java.util.List;

import io.socket.client.IO;
import io.socket.client.Socket;
import io.socket.emitter.Emitter;

/**
 * Created by Junhong on 2016-01-20.
 */
public class ImgSlideActivity extends Activity {
    private ViewPager viewPager;
    private final String TAG = "ImgSlideActivity";

    private String username;
    private String roomnumber;
    private String filename;
    private String timestamp;

    private ListItem[] list_item;
    private String[] img_urls;

    private Socket mSocket;
    {
        try {
            mSocket = IO.socket(Constants.CHAT_SERVER_URL);
        } catch (URISyntaxException e) {
            throw new RuntimeException(e);
        }
    }

    @Override
    protected void onCreate(Bundle savedInstanceState){
        super.onCreate(savedInstanceState);
        sendRequest();

        mSocket.on("versionResponse", onVersion);
    }

    public void sendRequest(){
        Intent load = getIntent();
        Bundle bundle = load.getExtras();
        username = (String)bundle.get("username");
        roomnumber = (String)bundle.get("position");
        filename = (String)bundle.get("filename");
        timestamp = (String)bundle.get("timestamp");

        Log.i(TAG, "onCreate " + username + " " + roomnumber + " " + filename + " " + timestamp);

        JSONObject request = new JSONObject();
        try {
            request.put("username", username);
            request.put("position", roomnumber);
            request.put("filename", filename);
            request.put("timestamp", timestamp);
        } catch (JSONException e) {
            e.printStackTrace();
        }

        mSocket.emit("version", request);
    }

    @Override
    protected void onDestroy(){
        super.onDestroy();
        mSocket.off("versionResponse", onVersion);
    }

    private Emitter.Listener onVersion = new Emitter.Listener(){

        @Override
        public void call(Object... args) {
            JSONArray data_arr = (JSONArray) args[0];

            list_item = new ListItem[data_arr.length()];
            img_urls = new String[data_arr.length()];

            for(int i = 0; i < data_arr.length(); i++){
                list_item[i] = new ListItem();
                img_urls[i] = new String();

                try {
                    JSONObject json = data_arr.getJSONObject(i);
                    list_item[i].setAuthor(json.getString("username"));
                    list_item[i].setImg_url(json.getString("html_addr"));
                    img_urls[i] = json.getString("html_addr");
                    list_item[i].setTimestamp(json.getString("time"));
                    list_item[i].setRoomnumber(json.getString("position"));
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }


        }
    };
}
