#define UNITY_ANDROID // for IDE

using UnityEngine;

namespace MainCore.Native
{
    public static class AndroidNativeInterface
    {
        private static bool _initialized;

        public static void Initialize()
        {
#if UNITY_ANDROID
            _androidJavaClass = new AndroidJavaClass(JavaClassName);
#endif
            _initialized = true;
        }

        public static string GetSharedFile()
        {
            if (!_initialized) return null;

#if UNITY_ANDROID
            return _androidJavaClass.CallStatic<string>("getSharedFile");
#else
            return null;
#endif
        }

        public static void RemoveSharedFile()
        {
            if (!_initialized) return;

#if UNITY_ANDROID
            _androidJavaClass.CallStatic("removeSharedFile");
#endif
        }

#if UNITY_ANDROID
        private static AndroidJavaClass _androidJavaClass;
        private const string JavaClassName = "com.totorowldox.REPhityOS.AndroidInterface";
#endif
    }
}