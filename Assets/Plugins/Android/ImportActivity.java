package com.totorowldox.REPhityOS;

import com.unity3d.player.UnityPlayerActivity;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.Window;
import android.widget.Toast;
import android.util.Log;
import android.net.Uri;
import android.database.Cursor;
import android.provider.OpenableColumns;

import com.totorowldox.REPhityOS.AndroidInterface;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.IOException;

public class ImportActivity extends Activity
{
    private static final String TAG = "ImportActivity";
    
    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        super.onCreate(savedInstanceState);
        
        detectIntent();

        Intent gameLoadIntent = new Intent(this, UnityPlayerActivity.class);
        this.startActivity(gameLoadIntent);
        this.finish();
    }

    @Override
    protected void onResume() 
    {
        detectIntent();
        super.onResume();
    }

    @Override
    public void onWindowFocusChanged(boolean hasFocus) 
    {
        if (hasFocus) detectIntent();
        super.onWindowFocusChanged(hasFocus);
    }

    @Override
    protected void onStart() 
    {
        detectIntent();
        super.onStart();
    }

    private void detectIntent()
    {
        try
        {
            Log.v(TAG, "detect intent");
            detectIntentInternal();
        }
        catch (Exception ex)
        {
            ex.printStackTrace();
        }
    }

    private void detectIntentInternal() throws IOException
    {
        if (AndroidInterface.getSharedFile() != null) return;
        
        Uri uri = getIntent().getData();
        if (uri == null) return;
        
        if (!uri.getScheme().equals("content")) 
        {
            Log.v(TAG, "non-content scheme");
            AndroidInterface.setSharedFile(uri.toString());
            return;
        }
        
        String fileName = getFileNameFromContentUri(uri);
        String extension = getExtension(fileName);
        if (extension == null) extension = ".zip";
        
        File targetFile = new File(getCacheDir(), "copied_import_cache" + extension);
        targetFile.createNewFile();
        
        FileOutputStream outStream = new FileOutputStream(targetFile);
        InputStream inStream = getContentResolver().openInputStream(uri);
        
        Log.v(TAG, "copy file to cache");
        try
        {
            copyStream(inStream, outStream);
        }
        catch (Exception ex)
        {
            ex.printStackTrace();
        }
        finally 
        {
            inStream.close();
            outStream.flush();
            outStream.close();
        }
        
        AndroidInterface.setSharedFile(targetFile.getAbsolutePath());
    }
    
    private String getExtension(String path)
    {
        int extIndex = path.lastIndexOf("."); 
        return extIndex == -1 ? null : path.substring(extIndex);
    }
    
    private String getFileNameFromContentUri(Uri uri)
    {
        Cursor cursor = null;
        try
        {
            cursor = getContentResolver().query(uri, null, null, null, null);
            int nameIndex = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME);
            cursor.moveToFirst();
            return cursor.getString(nameIndex);
        }
        finally
        {
            if (cursor != null) cursor.close();
        }
    }
    
    private void copyStream(InputStream inStream, OutputStream outStream) throws IOException
    {
        byte[] b = new byte[1024];
        int len;
        while ((len = inStream.read(b, 0, 1024)) > 0)
        {
            outStream.write(b, 0, len);
        }
    }
}