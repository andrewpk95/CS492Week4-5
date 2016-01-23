package com.passion.hamfeed;

/**
 * Created by Junhong on 2016-01-23.
 */
public class ReplyItem {
    //Do I need this strcutrue ?
    //key information inorder to
    private String author;
    private String img_url;
    private String timestamp;
    private String roomnumber;

    private String reply_user;
    private String reply_content;

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

    public void setReply_user(String rep_user){
        reply_user = rep_user;
    }

    public void setReply_content(String rep_cont){
        reply_content = rep_cont;
    }

    public String getReply_user(){
        return reply_user;
    }

    public String getReply_content(){
        return reply_content;
    }
}
