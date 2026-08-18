namespace MainCore.Utilities
{
    internal class BindingJVM
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        public static string GetFileDir()
        {
            using var javaClass = new UnityEngine.AndroidJavaClass("com.totorowldox.REPhityOS.PathUtil");
            return javaClass.CallStatic<string>("getFileDir");
        }
#else
        public static string GetFileDir()
        {
            return "";
        }
#endif
    }
}
