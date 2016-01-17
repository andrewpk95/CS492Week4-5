package com.passion.hamfeed;

import android.app.Activity;

import android.content.ActivityNotFoundException;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.provider.MediaStore;
import android.support.v4.app.Fragment;
import android.support.v4.content.CursorLoader;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.KeyEvent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.inputmethod.EditorInfo;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;
import com.koushikdutta.ion.Response;

import org.json.JSONObject;

import java.io.File;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.Future;

import io.socket.client.IO;
import io.socket.client.Socket;

/**
 * Created by Junhong on 2016-01-17.
 */
public class ImgFragment extends Fragment {

    private String TAG = "ImgFragment";    //to debug
    private Button send_img_sever, upload_img;
    private String img_path;
    private ImageView show_img;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setHasOptionsMenu(true);
        Ion.getDefault(getActivity()).configure().setLogging("ion-sample", Log.DEBUG);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_list, container, false);
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
                if(img_path == null){

                }else{
                    File img_file= new File(img_path);

                    Future uploading = Ion.with(getContext())
                            .load(Constants.IMG_SERVER_URL)
                            .setMultipartFile("image", img_file)
                            .asString()
                            .withResponse()
                            .setCallback(new FutureCallback<Response<String>>() {
                                @Override
                                public void onCompleted(Exception e, Response<String> result) {
                                    try{
                                        Log.i(TAG, "onCompleted " + result.getResult());
                                        JSONObject json = new JSONObject(result.getResult());
                                        Toast.makeText(getContext(), json.getString("response"), Toast.LENGTH_SHORT).show();
                                    }catch(Exception e1){
                                        e1.printStackTrace();
                                    }
                                }
                            });
                }
            }
        });

        show_img = (ImageView)view.findViewById(R.id.show_img);
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        Log.i(TAG, "onActivityResult");

        if(data == null){
            Log.i(TAG, "data is null");
            return;
        }

        if (Activity.RESULT_OK != resultCode) {
            Log.i(TAG, "Activity.Result_ok != resultCode");
            getActivity().finish();
            return;
        }

        if(Constants.SELECT_IMG != requestCode){
            Log.i(TAG, "gallerry is not returned well ");
            return;
        }

        img_path = getPathFromURI(data.getData());
        Log.i(TAG, "image path is " + img_path);
        show_img.setImageURI(data.getData());
    }

    private String getPathFromURI(Uri contentUri) {
        String[] proj = { MediaStore.Images.Media.DATA };
        CursorLoader loader = new CursorLoader(getContext(), contentUri, proj, null, null, null);
        Cursor cursor = loader.loadInBackground();
        int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
        cursor.moveToFirst();
        return cursor.getString(column_index);
    }
}
