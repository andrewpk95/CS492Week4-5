package com.passion.hamfeed;

/**
 * Created by Junhong on 2016-01-19.
 */
public class ListItem {
    private String author;
    private String img_url;
    private String timestamp;

    public void setAuthor(String auth){
        author = auth;
    }

    public String getAuthor(){
        return author;
    }

    public void setImg_url(String url){
        img_url = url;
    }

    public String getImg_url(){
        return img_url;
    }

    public void setTimestamp(String ts){
        timestamp = ts;
    }

    public String getTimestamp(){
        return timestamp;
    }
}
