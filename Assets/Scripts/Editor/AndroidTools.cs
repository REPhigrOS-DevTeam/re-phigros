#if UNITY_EDITOR && UNITY_ANDROID

using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public class KeystoreMatcher
    {
        static KeystoreMatcher()
        {
            try
            {
                PlayerSettings.Android.keystorePass = "Totorowldox";
                PlayerSettings.Android.keyaliasName = "greenball233_rpgr";
                PlayerSettings.Android.keyaliasPass = "Kagari939!!!";
            }
            catch (Exception)
            {
                Debug.LogError("Failed to match Android Keystore.");
            }
        }
    }
}

#endif