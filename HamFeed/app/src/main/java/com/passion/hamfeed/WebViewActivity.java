package com.passion.hamfeed;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.webkit.WebView;

/**
 * Created by Junhong on 2016-01-24.
 */
public class WebViewActivity extends Activity {
    private WebView webView;
    private String url;
    private String TAG = "WebViewActivity";

    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.webview);

        Intent intent = getIntent();
        if(intent == null){
            Log.i(TAG, "intent is null");
            finish();
        }else{
            url = intent.getStringExtra("URL");
            Log.i(TAG, url);
        }

        webView = (WebView) findViewById(R.id.webView);
        webView.getSettings().setBuiltInZoomControls(true);
        webView.getSettings().setJavaScriptEnabled(true);
        webView.loadUrl(url);

    }
}
