using UnityEditor;

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