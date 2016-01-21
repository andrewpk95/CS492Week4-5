package com.passion.hamfeed;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.media.Image;
import android.os.*;
import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentActivity;

import android.support.v4.app.FragmentManager;
import android.support.v4.app.FragmentPagerAdapter;
import android.support.v4.view.ViewPager;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.ListView;

import com.squareup.picasso.Picasso;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import io.socket.client.IO;
import io.socket.client.Socket;
import io.socket.emitter.Emitter;

/**
 * Created by Junhong on 2016-01-20.
 */
public class ImgSlideActivity extends FragmentActivity {
    private static final String TAG = "ImgSlideActivity";

    private String username;
    private String roomnumber;
    private String filename;
    private String timestamp;

    private static ListItem[] list_item;
    private static String[] img_urls;

    private Socket mSocket;
    {
        try {
            mSocket = IO.socket(Constants.CHAT_SERVER_URL);
        } catch (URISyntaxException e) {
            throw new RuntimeException(e);
        }
    }

    private static int swipe_length;
    private ImageFragmentPagerAdapter imageFragmentPagerAdapter;
    private ViewPager viewPager;
    private static Context mContext;

    public static final String[] IMAGE_NAME = {"eagle", "horse", "bonobo", "wolf", "owl", "bear",};
    private MsgHandler msgHandler;

    private final int FEED = 0;

    private ImageButton reply_button;
    private ListView reply_lv;

    @Override
    protected void onCreate(Bundle savedInstanceState){
        super.onCreate(savedInstanceState);
        Log.i(TAG, "onCreate 뭐하니?");
        setContentView(R.layout.fragment_page);
        imageFragmentPagerAdapter = new ImageFragmentPagerAdapter(getSupportFragmentManager());
        viewPager = (ViewPager)findViewById(R.id.pager);
        reply_button = (ImageButton)findViewById(R.id.reply_button);

        reply_lv = (ListView)findViewById(R.id.replyList);

        mContext = getApplicationContext();
        sendRequest();

        mSocket.on("versionResponse", onVersion);
        msgHandler = new MsgHandler();
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

            swipe_length = data_arr.length();

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

            msgHandler.sendEmptyMessage(FEED);
        }
    };

    public static class ImageFragmentPagerAdapter extends FragmentPagerAdapter {
        public ImageFragmentPagerAdapter(FragmentManager fm){
            super(fm);
        }

        @Override
        public Fragment getItem(int position) {
            SwipeFragment fragment = new SwipeFragment();
            return SwipeFragment.newInstance(position);
        }

        @Override
        public int getCount() {
            return swipe_length;
//            return 6;
        }
    }

    public static class SwipeFragment extends Fragment{
        @Override
        public View onCreateView(LayoutInflater inflater, ViewGroup container,
                                 Bundle savedInstanceState) {
            Log.i(TAG, "SwipeFragment onCreateView");
            View swipeView = inflater.inflate(R.layout.swipe_fragment, container, false);
            ImageView imageView = (ImageView) swipeView.findViewById(R.id.imageView);
            Bundle bundle = getArguments();
            int position = bundle.getInt("position");
//            loading image from server
            Picasso.with(mContext).load(img_urls[position]).into(imageView);
//            String imageFileName = IMAGE_NAME[position];
//            int imgResId = getResources().getIdentifier(imageFileName, "drawable", "com.passion.hamfeed");
//            imageView.setImageResource(imgResId);
            return swipeView;
        }

        static SwipeFragment newInstance(int position) {
            Log.i(TAG, "SwipeFragment newInstance");
            SwipeFragment swipeFragment = new SwipeFragment();
            Bundle bundle = new Bundle();
            bundle.putInt("position", position);
            swipeFragment.setArguments(bundle);
            return swipeFragment;
        }
    }

    class MsgHandler extends Handler {
        @Override
        public void handleMessage(android.os.Message msg){
            super.handleMessage(msg);

            switch(msg.what){
//                case FEED_LISTVIEW:
//                    Log.i(TAG, listItems.toString());
//
//                    ArrayList<ListItem> srcList = new ArrayList<ListItem>(Arrays.asList(listItems));
//                    lv.setAdapter(new CustomListAdapter(mContext, srcList));
                case FEED:
                    Log.i(TAG, "MsgHandler feed");
                    viewPager.setAdapter(imageFragmentPagerAdapter);
                    break;

            }
        }
    }
}
