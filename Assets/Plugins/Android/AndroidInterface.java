package com.totorowldox.REPhityOS;

import java.io.File;

public class AndroidInterface
{
    private static String sharedFile = null;
    
    public static String getSharedFile() 
    {
        return sharedFile;
    }
    
    public static void setSharedFile(String val)
    {
        sharedFile = val;
    }
    
    public static void removeSharedFile()
    {
        if (sharedFile == null) return;
        
        if (sharedFile.contains("copied_import_cache"))
        {
            File file = new File(sharedFile);
            file.delete();
        }
        
        sharedFile = null;
    }
}