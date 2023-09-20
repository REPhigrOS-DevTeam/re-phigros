using System;
using UnityEngine.Android;
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;
#endif

public class CheckAndroidPermission
{
// check skd >= 30 是否有外部存储读写权限 
    public static bool CheckFilePermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
        int sdkInt = buildVersion.GetStatic<int>("SDK_INT");

        AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");
        bool isExternalStorageManager = environment.CallStatic<bool>("isExternalStorageManager");

        if (sdkInt < 30 || isExternalStorageManager)
        {
            Debug.Log("已获得所有权限");
            return true;
        }

        return false;
#else
        return true;
#endif
    }

    // open all file access settings dialogue
    public static void OpenAllFilesAccessSettings(Action<string> onGranted = null, Action<string> onDenied = null, Action<string> onDeniedAndDontAskAgain = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        PermissionCallbacks permissionCallbacks = new PermissionCallbacks();
        permissionCallbacks.PermissionGranted += onGranted;
        permissionCallbacks.PermissionDenied += onDenied;
        permissionCallbacks.PermissionDeniedAndDontAskAgain += onDeniedAndDontAskAgain;
        Permission.RequestUserPermission("android.permission.MANAGE_ALL_FILES_ACCESS_PERMISSION", permissionCallbacks);
#endif
    }
}