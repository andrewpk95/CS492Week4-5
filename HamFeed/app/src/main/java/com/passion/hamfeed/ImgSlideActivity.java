package com.passion.hamfeed;

import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
import android.os.*;


import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentActivity;
import android.support.v4.app.FragmentManager;
import android.support.v4.app.FragmentPagerAdapter;
import android.support.v4.app.FragmentTransaction;
import android.support.v4.view.ViewPager;
import android.text.TextUtils;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import com.squareup.picasso.Picasso;
import com.viewpagerindicator.CirclePageIndicator;
import com.viewpagerindicator.TitlePageIndicator;
import com.viewpagerindicator.UnderlinePageIndicator;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.net.URISyntaxException;
import java.text.SimpleDateFormat;
import java.util.Date;

import io.socket.client.IO;
import io.socket.client.Socket;
import io.socket.emitter.Emitter;

/**
 * Created by Junhong on 2016-01-20.
 */
public class ImgSlideActivity extends FragmentActivity {
    private static final String TAG = "ImgSlideActivity";

    // this is for the version tracked image
    private static String username;
    private static String roomnumber;
    private static String filename;
    private static String timestamp;

    // this is for the current user
    private static String reply_user;
    private static String reply_time;

    private static ListItem[] list_item;
    private static String[] img_urls;

    private static Socket mSocket;
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
    private static MsgHandler msgHandler;

    private final int FEED = 0;
    private final int REPLY = 1;
    private static final int SEND_LIST_FEED = 2;

    private static Button webView;
    private TitlePageIndicator mIndicator;


    @Override
    protected void onCreate(Bundle savedInstanceState){
        super.onCreate(savedInstanceState);
        setContentView(R.layout.fragment_page);
        imageFragmentPagerAdapter = new ImageFragmentPagerAdapter(getSupportFragmentManager());
        viewPager = (ViewPager)findViewById(R.id.pager);
        mIndicator = (TitlePageIndicator)findViewById(R.id.indicator);

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
        reply_user = (String)bundle.get("replyuser");

//        Log.i(TAG, "onCreate image" + username + " " + roomnumber + " " + filename + " " + timestamp);
//        Log.i(TAG, "current user name " + reply_user);

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
            Log.i(TAG, "data_arr " + data_arr.toString());
            list_item = new ListItem[data_arr.length()];
            img_urls = new String[data_arr.length()];

            swipe_length = data_arr.length();

            int index = data_arr.length() - 1;

            for(int i = 0; i < data_arr.length(); i++){
                list_item[index - i] = new ListItem();
                img_urls[index - i] = new String();

                try {
                    JSONObject json = data_arr.getJSONObject(i);
                    Log.i(TAG, i + " 번째 " + json.toString());
                    list_item[index - i].setAuthor(json.getString("username"));
                    list_item[index - i].setImg_url(json.getString("html_addr"));
                    img_urls[index - i] = json.getString("html_addr");
                    list_item[index - i].setTimestamp(json.getString("time"));
                    list_item[index - i].setRoomnumber(json.getString("position"));
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
        }
    }

    public static class SwipeFragment extends Fragment{
        private TextView author;
        private TextView file;
        private TextView time;

        private ImageButton reply_button;
        private EditText reply;
        private ListView reply_lv;
        private JSONObject[] img_info;
        private ImageView imageView;
        private int card_position;

        @Override
        public void onCreate(Bundle savedInstanceState){
            super.onCreate(savedInstanceState);
            makeJsonArr();
        }

        @Override
        public View onCreateView(LayoutInflater inflater, ViewGroup container,
                                 Bundle savedInstanceState) {
//            Log.i(TAG, "SwipeFragment onCreateView");
            View swipeView = inflater.inflate(R.layout.swipe_fragment, container, false);
            imageView = (ImageView) swipeView.findViewById(R.id.imageView);
            Bundle bundle = getArguments();
            card_position = bundle.getInt("position");
//            Log.i(TAG, " " + card_position);
            //loading image from server
            Picasso.with(mContext).load(img_urls[card_position]).into(imageView);

            author = (TextView)swipeView.findViewById(R.id.author_name);
            file = (TextView)swipeView.findViewById(R.id.file_name);
            time = (TextView)swipeView.findViewById(R.id.when_upload);

            author.setText(list_item[card_position].getAuthor());
            file.setText(list_item[card_position].getFileName());
            time.setText(list_item[card_position].getTimestamp());

            imageView.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    showDialog(card_position);
                }
            });

            webView = (Button)swipeView.findViewById(R.id.webViewBtn);
            webView.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    ClipboardManager clipboardManager = (ClipboardManager)getActivity().getSystemService(CLIPBOARD_SERVICE);
                    ClipData clipData = ClipData.newPlainText("url", list_item[card_position].getImg_url());
                    clipboardManager.setPrimaryClip(clipData);
                    Toast.makeText(getContext(), "Web address is copied on Clipboard", Toast.LENGTH_SHORT).show();

                    Intent intent = new Intent(getContext(), WebViewActivity.class);
                    intent.putExtra("URL", list_item[card_position].getImg_url());
                    startActivity(intent);
                }
            });
            return swipeView;
        }

        public void makeJsonArr(){
            img_info = new JSONObject[list_item.length];
            for(int i = 0; i < list_item.length; i++){
                img_info[i] = new JSONObject();

                try {
                    img_info[i].put("filename", list_item[i].getFileName());
                    img_info[i].put("roomnumber", list_item[i].getRoomnumber());
                    img_info[i].put("time", list_item[i].getTimestamp());
                    img_info[i].put("username", list_item[i].getAuthor());
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
        }

        @Override
        public void onViewCreated(View view, Bundle savedInstanceState) {
            super.onViewCreated(view, savedInstanceState);
            reply_button = (ImageButton)view.findViewById(R.id.reply_button);
            reply = (EditText)view.findViewById(R.id.reply_input_msg);
            reply_button.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    //simultaneosuly bring back the data base and update the listview
                    attemptSend();
                }
            });
//            reply_lv = (ListView)view.findViewById(R.id.reply_list);
        }

        public void attemptSend(){
            String reply_msg = reply.getText().toString().trim();
            if(TextUtils.isEmpty(reply_msg)){
//                Log.i(TAG, "empty?? 비었다고?? " + reply_msg);
                reply.requestFocus();
                return;
            }
            reply.setText("");

            SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd_HHmmss");
            reply_time = sdf.format(new Date());

            JSONObject reply_json = new JSONObject();
            try {
                //the original image information
                reply_json.put("username", username);
                reply_json.put("roomnumber", roomnumber);
                reply_json.put("filename", filename);
                reply_json.put("time", timestamp);

                //reply for the image
                reply_json.put("replyuser", reply_user);
                reply_json.put("replymsg", reply_msg);
                reply_json.put("replytimestamp", reply_time);
            } catch (JSONException e) {
                e.printStackTrace();
            }

//            Log.i(TAG, "보내는거야?");
            mSocket.emit("reply", reply_json);
            msgHandler.sendEmptyMessage(SEND_LIST_FEED);
        }

        static SwipeFragment newInstance(int position) {
            SwipeFragment swipeFragment = new SwipeFragment();
            Bundle bundle = new Bundle();
            bundle.putInt("position", position);
            swipeFragment.setArguments(bundle);
            return swipeFragment;
        }

        public void showDialog(int pos){
            FragmentManager fm = getActivity().getSupportFragmentManager();
            ReplyDialogFragment reply_dialog = new ReplyDialogFragment();
            reply_dialog.setQuery(img_info[pos]);
            reply_dialog.show(fm, "dialog");
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
                    viewPager.setAdapter(imageFragmentPagerAdapter);
                    mIndicator.setViewPager(viewPager);
                    break;
                case REPLY:
                    Log.i(TAG, "reply view is updated");
                    break;
                case SEND_LIST_FEED:
                    mSocket.emit("replyReqeust", "PlzSendMe");
                    break;

            }
        }
    }
}
