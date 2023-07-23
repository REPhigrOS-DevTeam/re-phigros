#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

namespace MainCore.Utilities
{
    public class Util
    {
        public static void QuitApp()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}