package com.passion.hamfeed;

import java.io.Serializable;

/**
 * Created by Junhong on 2016-01-19.
 */
public class ListItem implements Serializable {
    private String author;
    private String img_url;
    private String timestamp;
    private String roomnumber;

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

    public void setRoomnumber(String proom){
        roomnumber = proom;
    }

    public String getRoomnumber(){
        return roomnumber;
    }

    public String getFileName(){
        String img_uuid = this.img_url.substring(img_url.lastIndexOf("/") + 1);
        String filename = img_uuid.substring(img_uuid.lastIndexOf(timestamp) + timestamp.length() + 1);
        return filename;
    }
}
