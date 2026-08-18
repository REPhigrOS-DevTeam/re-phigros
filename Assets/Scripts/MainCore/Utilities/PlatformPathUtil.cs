using System.IO;
using UnityEngine;

namespace MainCore.Utilities
{
    public class PlatformPathUtil
    {
        public static string StoragePath => Application.persistentDataPath;

        public static string TemporaryCachePath => Application.temporaryCachePath;

        public static string PrivatePath
        {
            get
            {
#if !UNITY_EDITOR && UNITY_IOS
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(home, "Library", "Preferences");
#elif !UNITY_EDITOR && UNITY_ANDROID
                return PathUtil.AndroidFileDir;
#else
                return Path.Combine(StoragePath, "private");
#if UNITY_EDITOR // 防止using优化被优化掉
                return PathUtil.AndroidFileDir;
#endif
#endif
            }
        }
    }
}
