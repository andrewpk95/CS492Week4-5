package com.passion.hamfeed;


import android.os.Bundle;
import android.os.Handler;
import android.support.v4.app.DialogFragment;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.net.URISyntaxException;

import io.socket.client.IO;
import io.socket.client.Socket;
import io.socket.emitter.Emitter;

/**
 * Created by Junhong on 2016-01-23.
 */
public class ReplyDialogFragment extends DialogFragment implements AdapterView.OnItemClickListener {
    private String[] listitems = { "item01", "item02", "item03", "item04" };
    private ListView mylist;
    private String TAG = "ReplyDialogFragment";
    private JSONObject query;
    private static MsgHandler msgHandler;
    private final int LISTVIEW = 0;
    private String[] reply_items;

    private static Socket mSocket;
    {
        try {
            mSocket = IO.socket(Constants.CHAT_SERVER_URL);
        } catch (URISyntaxException e) {
            throw new RuntimeException(e);
        }
    }

    public void setQuery(JSONObject pquery){
        query = pquery;
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        Log.i(TAG, "QUERY : " + query.toString());
        mSocket.emit("replyRequest", query);
        mSocket.on("replyResponse", onReply);

        View view = inflater.inflate(R.layout.dialog_fragment, null, false);
        mylist = (ListView) view.findViewById(R.id.reply_list);

        msgHandler = new MsgHandler();
        getDialog().getWindow().requestFeature(Window.FEATURE_NO_TITLE);
        return view;
    }

    @Override
    public void onDestroy(){
        super.onDestroy();
        mSocket.off("replyResponse", onReply);
    }

    @Override
    public void onActivityCreated(Bundle savedInstanceState) {

        super.onActivityCreated(savedInstanceState);

        mylist.setOnItemClickListener(this);

    }

    @Override
    public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
        dismiss();
    }

    private Emitter.Listener onReply = new Emitter.Listener(){

        @Override
        public void call(Object... args) {
            JSONArray data_arr = (JSONArray) args[0];
            Log.i(TAG, data_arr.toString());

            reply_items = new String[data_arr.length()];
            for(int i = 0; i < data_arr.length(); i++){
                reply_items[i] = new String();
                try {
                    JSONObject json = data_arr.getJSONObject(i);
                    reply_items[i] = json.getString("replymsg");
                } catch (JSONException e) {
                    e.printStackTrace();
                }
                Log.i(TAG, i + " 번째 " + reply_items[i]);
            }

            msgHandler.sendEmptyMessage(LISTVIEW);
        }
    };

    class MsgHandler extends Handler {
        @Override
        public void handleMessage(android.os.Message msg){
            super.handleMessage(msg);

            switch(msg.what){
                case LISTVIEW:
                    ArrayAdapter<String> adapter = new ArrayAdapter<String>(getActivity(),
                            android.R.layout.simple_list_item_1, reply_items);

                    mylist.setAdapter(adapter);
                    break;
            }
        }
    }
}
