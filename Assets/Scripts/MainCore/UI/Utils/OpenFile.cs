using System;
using System.Collections.Generic;
using SFB;
using SimpleFileBrowser;

namespace MainCore.UI.Utils
{
    public class OpenFile
    {
        // 妈的日了狗了传中文会乱码
        public static void LoadFolder(Action<string> onSuccess, FileBrowser.OnCancel onCancel, string initPath,
            string title, string buttonText)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            try
            {
                string[] s = StandaloneFileBrowser.OpenFolderPanel(title, initPath, false);
                if (s.Length == 0)
                {
                    onCancel.Invoke();
                }
                else
                {
                    onSuccess.Invoke(s[0]);
                }
            }
            catch (Exception)
            {
                onCancel.Invoke();
            }

            return;
#endif
            FileBrowser.SetFilters(true);
            FileBrowser.ShowLoadDialog(paths => onSuccess.Invoke(paths[0]), onCancel, FileBrowser.PickMode.Folders,
                false, initPath, "", title,
                buttonText);
        }

        public static void LoadFile(Action<string> onSuccess, FileBrowser.OnCancel onCancel, ExtensionFilter[] filter,
            string initPath, string title, string buttonText)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            try
            {
                string[] openFilePanel = StandaloneFileBrowser.OpenFilePanel(title, initPath, filter, false);
                if (openFilePanel.Length == 0)
                {
                    onCancel.Invoke();
                }
                else
                {
                    onSuccess.Invoke(openFilePanel[0]);
                }
            }
            catch (Exception)
            {
                onCancel.Invoke();
            }

            return;
#endif
            FileBrowser.SetFilters(false, ParseSimpleFileBrowserFilter(filter));
            FileBrowser.ShowLoadDialog(paths => onSuccess.Invoke(paths[0]), onCancel, FileBrowser.PickMode.Files, false,
                initPath, "", title,
                buttonText);
        }

        public static void SaveFile(Action<string> onSuccess, FileBrowser.OnCancel onCancel, ExtensionFilter[] filter,
            string initPath, string title, string buttonText)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            try
            {
                string s = StandaloneFileBrowser.SaveFilePanel(title, initPath, "", filter);
                if (string.IsNullOrEmpty(s))
                {
                    onCancel.Invoke();
                }
                else
                {
                    onSuccess.Invoke(s);
                }
            }
            catch (Exception)
            {
                onCancel.Invoke();
            }

            return;
#endif
            FileBrowser.SetFilters(false, ParseSimpleFileBrowserFilter(filter));
            FileBrowser.ShowSaveDialog(paths => onSuccess.Invoke(paths[0]), onCancel, FileBrowser.PickMode.Files, false,
                initPath, "", title,
                buttonText);
        }

        private static IEnumerable<string> ParseSimpleFileBrowserFilter(ExtensionFilter[] filter)
        {
            List<string> extensions = new List<string>();
            foreach (ExtensionFilter extensionFilter in filter)
            {
                extensions.AddRange(extensionFilter.Extensions);
            }

            return extensions;
        }
    }
}