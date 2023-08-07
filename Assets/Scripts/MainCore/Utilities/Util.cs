using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unimage;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

namespace MainCore.Utilities
{
    public static class Util
    {
        public static void QuitApp()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public static string GetMD5(byte[] data)
        {
            byte[] result = MD5.Create().ComputeHash(data);
            StringBuilder sBuilder = new StringBuilder();
            foreach (var t in result)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static bool IsSameOrSubClass(this Type type, Type type1)
        {
            return type.IsSubclassOf(type1) || type == type1;
        }

        public static Sprite ConvertFileToSprite(byte[] data)
        {
            try
            {
                using UnimageProcessor unimageProcessor = new UnimageProcessor();
                unimageProcessor.Load(data);
                Texture2D texture = unimageProcessor.GetTexture(noLongerReadable: false);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        public static async Task<AudioClip> ReadMusicAsAudioClip(string path)
        {
            string suffix = Path.GetExtension(path).ToLowerInvariant();
            Uri.TryCreate(path, UriKind.Absolute, out Uri uri);
            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uri, suffix switch
            {
                ".wav" => AudioType.WAV,
                ".ogg" => AudioType.OGGVORBIS,
                ".mp3" => AudioType.MPEG,
                _ => AudioType.UNKNOWN
            });
            await uwr.SendWebRequest();
            return DownloadHandlerAudioClip.GetContent(uwr);
        }
    }
}