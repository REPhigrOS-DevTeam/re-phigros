using System;
using System.IO;
using System.Security;
using JetBrains.Annotations;
using MainCore.Native;
using MainCore.UI;
using MainCore.Utilities;
using UnityEngine;

public class BridgeForNative : MonoBehaviour
{
    [SerializeField] private string debugFileUri;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home))
        {
            ProcessFilePath(debugFileUri, InGameUIManager.ShowModalWindowWithCloseFromWindowInfo);
        }
    }
#endif

    #region ForNative

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        CheckOutSharedFile();
#endif
    }

    [UsedImplicitly]
    private void CheckOutSharedFile()
    {
        Action<InGameUIManager.WindowInfo> logger = InGameUIManager.ShowModalWindowWithCloseFromWindowInfo;
#if UNITY_ANDROID && !UNITY_EDITOR
        var filePath = AndroidNativeInterface.GetSharedFile();
#else
        var filePath = GetFilePath(debugFileUri, logger);
#endif
        ProcessFilePath(filePath, logger);
        AndroidNativeInterface.RemoveSharedFile();
    }

#if UNITY_IOS && !UNITY_EDITOR || true
    [UsedImplicitly]
    public void Callback_GetiOSSharedFile(string uri)
    {
        ProcessFilePath(uri, InGameUIManager.ShowModalWindowWithCloseFromWindowInfo);
    }
#endif

    #endregion

    [UsedImplicitly]
    private void ProcessFilePath(string uri, Action<InGameUIManager.WindowInfo> logger)
    {
        string filePath = GetFilePath(uri, logger);
        if (string.IsNullOrEmpty(filePath)) return;
        
        Debug.Log("[Unity callback] File Path is: " + filePath);

        if (!File.Exists(filePath))
        {
            logger(new InGameUIManager.WindowInfo
            {
                title = "错误",
                content = "文件不存在",
                confirmAction = () => { },
                confirmText = "确定"
            });
            return;
        }

        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".zip" && extension != ".pez")
        {
            logger(new InGameUIManager.WindowInfo
            {
                title = "错误",
                content = string.IsNullOrEmpty(extension) ? "未知的格式" : $"不支持的格式：{extension}",
                confirmAction = () => { },
                confirmText = "确定"
            });
            return;
        }

        try
        {
            GameUtils.UnzipChartArchive(filePath, () =>
            {
                Camera mainCamera = Camera.main;
                if (mainCamera && mainCamera.TryGetComponent(out SelectUIControl selectUIControl))
                {
                    selectUIControl.RefreshGameFolder();
                }
            }, logger);
        }
        catch (IOException e)
        {
            Debug.LogException(e);
            logger(new InGameUIManager.WindowInfo
            {
                title = "错误",
                content = "IOException",
                confirmAction = () => { },
                confirmText = "确定"
            });
        }
        catch (SecurityException e)
        {
            Debug.LogException(e);
            logger(new InGameUIManager.WindowInfo
            {
                title = "错误",
                content = "SecurityException",
                confirmAction = () => { },
                confirmText = "确定"
            });
        }
        catch (UnauthorizedAccessException e)
        {
            Debug.LogException(e);
            logger(new InGameUIManager.WindowInfo
            {
                title = "错误",
                content = "UnauthorizedAccessException",
                confirmAction = () => { },
                confirmText = "确定"
            });
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            logger(new InGameUIManager.WindowInfo
            {
                title = "错误",
                content = $"奇怪的错误: {e.GetType().Name}",
                confirmAction = () => { },
                confirmText = "确定"
            });
        }
    }

    private string GetFilePath(string uri, Action<InGameUIManager.WindowInfo> logger)
    {
        if (string.IsNullOrEmpty(uri))
        {
            logger(new InGameUIManager.WindowInfo
            {
                title = "错误",
                content = "url为空",
                confirmAction = () => { },
                confirmText = "确定"
            });
            return "";
        }

        if (!uri.StartsWith("file://"))
        {
            int i = uri.IndexOf("://", StringComparison.Ordinal);
            logger(new InGameUIManager.WindowInfo
            {
                title = "错误",
                content = i < 0 ? "url协议为空" : $"未知的url协议：{uri[..i]}",
                confirmAction = () => { },
                confirmText = "确定"
            });
            return "";
        }

        return uri["file://".Length..];
    }
}