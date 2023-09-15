using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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

        public static Sprite ReadFileAsSprite(byte[] data, out Exception exception)
        {
            try
            {
                exception = null;
                using UnimageProcessor unimageProcessor = new UnimageProcessor();
                unimageProcessor.Load(data);
                Texture2D texture = unimageProcessor.GetTexture(noLongerReadable: false);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
            }
            catch (Exception e)
            {
                exception = e;
                return null;
            }
        }

        public static async Task<AudioClip> ReadMusicAsAudioClip(string path)
        {
            await UniTask.SwitchToMainThread();
            Uri.TryCreate(path, UriKind.Absolute, out Uri uri);
            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uri, GetAudioTypeFromFile(path));
            await uwr.SendWebRequest();
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
            return audioClip;
        }

        public static AudioType GetAudioTypeFromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            if (data.SplitByteArrayToString(3) == "ID3") return AudioType.MPEG;
            if (data.SplitByteArrayToString(4) == "OggS") return AudioType.OGGVORBIS;
            if (data.SplitByteArrayToString(4) == "RIFF") return AudioType.WAV;
            if (data.SplitByteArrayToString(4) == "fLaC") throw new ArgumentException();
            return AudioType.UNKNOWN;
        }

        private static string SplitByteArrayToString(this byte[] arr, int length)
        {
            return Encoding.ASCII.GetString(arr.Take(length).ToArray());
        }

        public static Color GetAvgColor(Sprite sprite)
        {
            if (!sprite.texture.isReadable) return Color.white;
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);
            float r = 0, g = 0, b = 0;
            foreach (var p in pixels)
            {
                if (p == new Color(0, 0, 0)) continue;
                r += p.r;
                g += p.g;
                b += p.b;
            }

            // Debug.Log($"{r}, {g}, {b}, {pixelCount}");
            r /= pixels.Length;
            g /= pixels.Length;
            b /= pixels.Length;
            // Debug.Log($"result: {r}, {g}, {b}");
            return new Color(r, g, b);
        }

        public static Color GetPossibleBGColor(Sprite sprite)
        {
            Color avgColor = GetAvgColor(sprite);
            Color.RGBToHSV(avgColor, out float h, out float s, out float v);
            h = (h + 0.5f) % 1f;
            s = 1 - s;
            Color color = Color.HSVToRGB(h, s, v);
            return new Color(color.r, color.g, color.b, avgColor.a);
        }
    }
}