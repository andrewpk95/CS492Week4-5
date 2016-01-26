package com.passion.hamfeed;

/**
 * Created by Junhong on 2016-01-14.
 */
public class Constants {
    public static final String CHAT_SERVER_URL = "http://143.248.140.92:1111";
    public static final String GAME_SERVER_URL = "http://143.248.140.92:2222";
    public static final String IMG_SERVER_URL = "http://143.248.140.92:1112/upload";
    public static final String IMG_INFOR_URL = "http://143.248.140.92:1112/info";
    public static int LOGIN_OK = 10;
    public static int SELECT_IMG = 11;

    public static final int PLAY_REQUEST = 12;

    //Soemthings required on chatting
    public static final int REQUEST_LOGIN = 0;
    public static final int TYPING_TIMER_LENGTH = 600;

    //json tags
    public static final String TAG_STATUS = "status";
    public static final String TAG_DATA = "data";
    public static final String TAG_PRODUCTS = "products";
    public static final String KEY_NAME = "name";
    public static final String KEY_IMAGE_URL = "image_url";
    public static final String KEY_ID = "id";
}
