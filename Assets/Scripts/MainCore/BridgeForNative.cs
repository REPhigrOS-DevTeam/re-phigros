using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using JetBrains.Annotations;
using MainCore.Native;
using MainCore.UI;
using MainCore.UI.Selection;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.Networking;

public class BridgeForNative : MonoBehaviour
{
    [SerializeField] private string debugFilePath;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home))
        {
            ProcessFilePath(File.Exists(debugFilePath) && Path.GetExtension(debugFilePath).ToLowerInvariant() is ".zip" or ".pez", debugFilePath, InGameUIManager.ShowModalWindowWithCloseFromWindowInfo);
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
        var filePath = debugFilePath;
#endif
        ProcessFilePath(filePath != null, filePath, logger);
        AndroidNativeInterface.RemoveSharedFile();
    }

#if UNITY_IOS && !UNITY_EDITOR || true
    [UsedImplicitly]
    public void Callback_GetiOSSharedFile(string str)
    {
        int indexOf = str.IndexOf('\n', StringComparison.Ordinal);
        bool state = bool.Parse(str[..indexOf]);
        string path = str[(indexOf + 1)..];
        Action<InGameUIManager.WindowInfo> logger = InGameUIManager.ShowModalWindowWithCloseFromWindowInfo;
        ProcessFilePath(state, GetFilePath(path, logger), logger);
    }
#endif

    #endregion

    [UsedImplicitly]
    private void ProcessFilePath(bool state, string filePath, Action<InGameUIManager.WindowInfo> logger)
    {
        if (!state)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger(new InGameUIManager.WindowInfo
                {
                    title = "错误",
                    content = "url为空",
                    confirmAction = () => { },
                    confirmText = "确定"
                });
            }
            else
            {
                logger(new InGameUIManager.WindowInfo
                {
                    title = "错误",
                    content = $"url或文件路径非法：{filePath}",
                    confirmAction = () => { },
                    confirmText = "确定"
                });
            }
            return;
        }

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
            if (!Directory.Exists(Application.temporaryCachePath)) Directory.CreateDirectory(Application.temporaryCachePath);
            string tmpFilePath = Path.Combine(Application.temporaryCachePath, Path.GetFileName(filePath)); // 防止丢失权限，存储到缓存里
            File.Copy(filePath, tmpFilePath, true);
            GameUtils.UnzipChartArchive(tmpFilePath, () =>
            {
                var sm = GameObject.Find("[Manager]").GetComponent<SelectionManager>();
                sm.RefreshGameFolder();
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

        return UnityWebRequest.UnEscapeURL(uri["file://".Length..]);
    }
}