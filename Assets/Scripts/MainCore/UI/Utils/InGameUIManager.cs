using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

public static class InGameUIManager
{
    private static ConcurrentQueue<WindowInfo> queue = new();
    public static bool IsActive => InGameModalWindow.Instance.IsActive;

    /// <summary>
    /// 打开一个模态窗口
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="content">内容</param>
    /// <param name="confirmAction">确认后的行动</param>
    /// /// <param name="confirmtext">确认按钮的text</param>
    /// <param name="cancelAction">取消后的行动</param>
    /// <param name="canceltext">取消按钮的text</param>
    public static void ShowModalWindow(string title, string content, Action confirmAction,
        string confirmtext = "Confirm", Action cancelAction = null, string canceltext = "Cancel",
        Action alternateAction = null, string alternatetext = "Alternate")
    {
        if (IsActive)
        {
            queue.Enqueue(new WindowInfo
            {
                withClose = false,
                title = title,
                content = content,
                confirmAction = confirmAction,
                confirmText = confirmtext,
                cancelAction = cancelAction,
                cancelText = canceltext,
                alternateAction = alternateAction,
                alternateText = alternatetext
            });
            return;
        }

        InGameModalWindow.Instance.Show(title, content, confirmAction, confirmtext, cancelAction, canceltext,
            alternateAction, alternatetext);
    }

    /// <summary>
    /// 打开一个模态窗口，一旦按下按钮就消失
    /// </summary>
    public static void ShowModalWindowWithClose(string title, string content, Action confirmAction,
        string confirmtext = "Confirm", Action cancelAction = null, string canceltext = "Cancel",
        Action alternateAction = null, string alternatetext = "Alternate")
    {
        if (IsActive) return;
        confirmAction += HideModalWindow;

        if (cancelAction != null)
        {
            cancelAction += HideModalWindow;
        }

        if (alternateAction != null)
        {
            alternateAction += HideModalWindow;
        }

        InGameModalWindow.Instance.Show(title, content, confirmAction, confirmtext, cancelAction, canceltext,
            alternateAction, alternatetext);
    }

    /// <summary>
    /// 隐藏模态窗口，无动画
    /// </summary>
    public static void HideModalWindowForcely()
    {
        InGameModalWindow.Instance.HideForcely();
    }

    /// <summary>
    /// 隐藏模态窗口
    /// </summary>
    public static void HideModalWindow()
    {
        InGameModalWindow.Instance.Hide();
    }

    public static void CheckWindowToShow()
    {
        if (queue.Count == 0) return;
        if (!queue.TryDequeue(out WindowInfo windowInfo)) return;
        if (windowInfo.withClose)
        {
            windowInfo.confirmAction += HideModalWindow;

            if (windowInfo.cancelAction != null)
            {
                windowInfo.cancelAction += HideModalWindow;
            }

            if (windowInfo.alternateAction != null)
            {
                windowInfo.alternateAction += HideModalWindow;
            }
        }

        InGameModalWindow.Instance.Show(windowInfo.title, windowInfo.content, windowInfo.confirmAction,
            windowInfo.confirmText, windowInfo.cancelAction, windowInfo.cancelText, windowInfo.alternateAction,
            windowInfo.alternateText);
    }

    public static IEnumerator Join()
    {
        yield return new WaitWhile(() => IsActive);
    }

    class WindowInfo
    {
        public bool withClose;
        public string title;
        public string content;
        public Action confirmAction;
        public string confirmText;
        public Action cancelAction;
        public string cancelText;
        public Action alternateAction;
        public string alternateText;
    }
}