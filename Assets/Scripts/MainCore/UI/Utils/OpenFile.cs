using System;
using System.Collections.Generic;
using System.Linq;
using Schwarzer.Windows;
using SimpleFileBrowser;

namespace MainCore.UI.Utils
{
    public class OpenFile
    {
        private static IEnumerable<string> ParseSimpleFileBrowserFilter(string filter)
        {
            return filter.Split("|").Where((_, i) => i % 2 == 1).Select(str => str[str.LastIndexOf('.')..]);
        }
        public static void LoadFolder(FileBrowser.OnSuccess onSuccess, FileBrowser.OnCancel onCancel, string initPath, string title, string buttonText)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            try
            {
                string s = Dialog.OpenFolderDialog(title, initPath);
                if (string.IsNullOrEmpty(s))
                {
                    onCancel.Invoke();
                }
                else
                {
                    onSuccess.Invoke(new[] { s });
                }
            }
            catch (Exception)
            {
                onCancel.Invoke();
            }
            return;
#endif
            FileBrowser.SetFilters(true);
            FileBrowser.ShowLoadDialog(onSuccess, onCancel, FileBrowser.PickMode.Folders, false, initPath, "", title,
                buttonText);
        }
        public static void LoadFile(FileBrowser.OnSuccess onSuccess, FileBrowser.OnCancel onCancel, string filter,
            string initPath, string title, string buttonText)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            try
            {
                string s = Dialog.OpenFileDialog(title, "", initPath, filter);
                if (string.IsNullOrEmpty(s))
                {
                    onCancel.Invoke();
                }
                else
                {
                    onSuccess.Invoke(new[] { s });
                }
            }
            catch (Exception)
            {
                onCancel.Invoke();
            }
            return;
#endif
            FileBrowser.SetFilters(false, ParseSimpleFileBrowserFilter(filter));
            FileBrowser.ShowLoadDialog(onSuccess, onCancel, FileBrowser.PickMode.Files, false, initPath, "", title,
                buttonText);
        }
        
        public static void SaveFile(FileBrowser.OnSuccess onSuccess, FileBrowser.OnCancel onCancel, string filter,
            string initPath, string title, string buttonText)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            try
            {
                string s = Dialog.SaveFileDialog(title, "", initPath, filter);
                if (string.IsNullOrEmpty(s))
                {
                    onCancel.Invoke();
                }
                else
                {
                    onSuccess.Invoke(new[] { s });
                }
            }
            catch (Exception)
            {
                onCancel.Invoke();
            }
            // return;
#endif
            FileBrowser.SetFilters(false, ParseSimpleFileBrowserFilter(filter));
            FileBrowser.ShowSaveDialog(onSuccess, onCancel, FileBrowser.PickMode.Files, false, initPath, "", title,
                buttonText);
        }
    }
}