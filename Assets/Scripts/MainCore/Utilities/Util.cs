using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MainCore.Data;
using MainCore.UI;
using Newtonsoft.Json;
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

        public static Texture2D ReadFileAsTexture(byte[] data)
        {
            using UnimageProcessor unimageProcessor = new UnimageProcessor();
            unimageProcessor.Load(data);
            return unimageProcessor.GetTexture(noLongerReadable: false);
        }

        public static Sprite ReadFileAsSprite(byte[] data, out Exception exception)
        {
            try
            {
                exception = null;
                Texture2D texture = ReadFileAsTexture(data);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 100f, 1);
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

        public static float GetRatio(this Vector2 vector2)
        {
            return vector2.x / vector2.y;
        }

        public static string FirstToLowerInvariant(this string str)
        {
            if (str.Length < 1) return str;
            return str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Gcd(int a, int b)
        {
            while (b > 0)
            {
                var r = a % b;
                a = b;
                b = r;
            }

            return a;
        }

        public static float Frac(this int[] frac)
        {
            if (frac.Length == 3)
            {
                if (frac.Length == 3) return frac[0] + (float)frac[1] / frac[2];
                return frac[0];
            }

            return frac.Length > 0 ? frac[0] : 0f;
        }

        public static Sprite ReadSprite(byte[] data, Vector2 pivot, float pixelsPerUnit = 100f)
        {
            return ReadSprite(ReadFileAsTexture(data), pivot, pixelsPerUnit);
        }

        public static Sprite ReadSprite(Texture2D texture2D, Vector2 pivot, float pixelsPerUnit = 100f)
        {
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), pivot, pixelsPerUnit, 1);
        }

        public static CharacterImage GetCharacterImage(string packagePath, Action onFileInvalidFound = null,
            Action onFormatInvalidFound = null)
        {
            byte[] pkgData;
            try
            {
                pkgData = FileEncryptor.Decrypt(File.ReadAllBytes(packagePath));
            }
            catch (IOException)
            {
                onFileInvalidFound?.Invoke();
                throw new ArgumentException();
            }
            catch (Exception)
            {
                onFormatInvalidFound?.Invoke();
                throw new ArgumentException();
            }

            return GetCharacterImage(pkgData, onFileInvalidFound, onFormatInvalidFound);
        }

        public static CharacterImage GetCharacterImage(byte[] data, Action onFileInvalidFound = null,
            Action onFormatInvalidFound = null)
        {
            string tmpDirPath = Application.temporaryCachePath + "/tmpCharaImage";
            if (Directory.Exists(tmpDirPath)) Directory.Delete(tmpDirPath, true);
            Directory.CreateDirectory(tmpDirPath);

            try
            {
                ZipUtils.UnZip(data, tmpDirPath);
            }
            catch (Exception)
            {
                onFileInvalidFound?.Invoke();
                throw new ArgumentException();
            }

            if (!File.Exists(tmpDirPath + "/chara") || !File.Exists(tmpDirPath + "/index.json") || !File.Exists(tmpDirPath + "/hash"))
            {
                onFormatInvalidFound?.Invoke();
                throw new ArgumentException();
            }

            string configStr;
            byte[] textureData = File.ReadAllBytes(tmpDirPath + "/chara");

            try
            {
                byte[] configData = File.ReadAllBytes(tmpDirPath + "/index.json");
                byte[] bytes = File.ReadAllBytes(tmpDirPath + "/hash");
                var decrypt = FileEncryptor.RsaDecrypt(bytes);
                if (decrypt.Length != 64) throw new ArgumentException();
                if (!decrypt.Take(32).ToArray().SequenceEqual(FileEncryptor.ComputeSha256(textureData)) || !decrypt.Skip(32).ToArray().SequenceEqual(FileEncryptor.ComputeSha256(configData))) throw new ArgumentException();
                configStr = Encoding.UTF8.GetString(configData);
            }
            catch (IOException)
            {
                onFileInvalidFound?.Invoke();
                throw new ArgumentException();
            }
            catch (Exception)
            {
                onFormatInvalidFound?.Invoke();
                throw new ArgumentException();
            }

            ExternalCharacterInfo externalCharacterInfo;
            try
            {
                externalCharacterInfo =
                    JsonConvert.DeserializeObject<ExternalCharacterInfo>(configStr);
            }
            catch (IOException)
            {
                onFileInvalidFound?.Invoke();
                throw new ArgumentException();
            }
            catch (Exception)
            {
                onFormatInvalidFound?.Invoke();
                throw new ArgumentException();
            }

            Directory.Delete(tmpDirPath, true);

            return new CharacterImage
            {
                TextureData = textureData,
                Info = externalCharacterInfo
            };
        }
        
        public static IEnumerable<long> IndexOfByBoyerMooreHorspool(this byte[] source, byte[] pattern, int start = 0)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            long valueLength = source.LongLength;
            long patternLength = pattern.LongLength;

            if ((valueLength == 0) || (patternLength == 0) || (patternLength > valueLength))
            {
                yield break;
            }

            var badCharacters = new long[256];

            for (var i = 0; i < 256; i++)
            {
                badCharacters[i] = patternLength;
            }

            var lastPatternByte = patternLength - 1;

            for (long i = 0; i < lastPatternByte; i++)
            {
                badCharacters[pattern[i]] = lastPatternByte - i;
            }

            long index = start;

            while (index <= valueLength - patternLength)
            {
                for (var i = lastPatternByte; source[index + i] == pattern[i]; i--)
                {
                    if (i == 0)
                    {
                        yield return index;
                        break;
                    }
                }

                index += badCharacters[source[index + lastPatternByte]];
            }
        }

        public static string DataPath => PlayerPrefs.GetString("file_path", Application.persistentDataPath);
    }
}