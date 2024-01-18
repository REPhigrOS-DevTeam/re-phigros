using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using MainCore.Data;
using MainCore.UI;
using Newtonsoft.Json;
using NLayer;
using Unimage;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
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

        public static async UniTask<Texture2D> ReadFileAsTextureAsync(byte[] data)
        {
            using UnimageProcessor unimageProcessor = new UnimageProcessor();
            await unimageProcessor.LoadAsync(data);
            await UniTask.SwitchToMainThread();
            return unimageProcessor.GetTexture(noLongerReadable: false);
        }

        public static (Sprite, Exception) ReadFileAsSprite(byte[] data, float ppu = 100f)
        {
            try
            {
                Texture2D texture = ReadFileAsTexture(data);
                return (Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), ppu, 1), null);
            }
            catch (Exception e)
            {
                return (null, e);
            }
        }

        public static async UniTask<(Sprite, Exception)> ReadFileAsSpriteAsync(byte[] data, float ppu = 100f)
        {
            try
            {
                Texture2D texture = await ReadFileAsTextureAsync(data);
                return (Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), ppu, 1), null);
            }
            catch (Exception e)
            {
                return (null, e);
            }
        }

        public static async UniTask<AudioClip> ReadMusicAsAudioClip(string path, string clipName = "",
            bool readAll = false)
        {
            AudioType? audioType = await GetAudioTypeFromFile(path);
            switch (audioType)
            {
                case null:
                    return null;
                case AudioType.MPEG:
                {
                    MpegFile mpegFile = new MpegFile(path);

                    if (readAll)
                    {
                        int lengthSamples = (int)(mpegFile.Length / sizeof(float) / mpegFile.Channels);
                        float[] samples = new float[lengthSamples * mpegFile.Channels];
                        int _ = mpegFile.ReadSamples(samples, 0, lengthSamples * mpegFile.Channels);
                        AudioClip ac = AudioClip.Create(clipName, lengthSamples, mpegFile.Channels, mpegFile.SampleRate,
                            false);
                        ac.SetData(samples, 0);
                        mpegFile.Dispose();
                        return ac;
                    }

                    AudioClip ac1 = AudioClip.Create(clipName,
                        (int)(mpegFile.Length / sizeof(float) / mpegFile.Channels),
                        mpegFile.Channels,
                        mpegFile.SampleRate,
                        true,
                        data =>
                        {
                            float[] f = new float[data.Length];
                            int _ = mpegFile.ReadSamples(f, 0, data.Length);
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = f[i];
                            }
                        },
                        position =>
                        {
                            mpegFile.Dispose();
                            mpegFile = new MpegFile(path);
                            mpegFile.Position = position * sizeof(float) * mpegFile.Channels;
                        }
                    );

                    return ac1;
                }
                case AudioType.OGGVORBIS:
                {
                    // Load the data into a stream
                
                    NVorbis.VorbisReader vorbis = new NVorbis.VorbisReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                    int samplecount = (int)(vorbis.TotalSamples / vorbis.Channels);
                    
                    if (readAll)
                    {
                        float[] samples = new float[vorbis.TotalSamples];
                        int _ = vorbis.ReadSamples(samples, 0, samples.Length);

                        AudioClip ac = AudioClip.Create(clipName, samplecount, vorbis.Channels, vorbis.SampleRate,
                            false);
                        ac.SetData(samples, 0);
                        vorbis.Dispose();
                        if (clipName == "click")
                        {
                            Debug.Log($"[{string.Join(", ", samples.Take(Mathf.Min(samples.Length, 20)))}]");
                        }
                        return ac;
                    }
                
                    AudioClip ac1 = AudioClip.Create(clipName, samplecount, vorbis.Channels, vorbis.SampleRate, false,
                        data =>
                        {
                            var f = new float[data.Length];
                            int _ = vorbis.ReadSamples(f, 0, data.Length);
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = f[i];
                            }
                        }, position =>
                        {
                            vorbis.Dispose();
                            vorbis = new NVorbis.VorbisReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                            int offset = (int)(vorbis.TotalSamples - (long)(vorbis.SampleRate * vorbis.TotalTime.TotalSeconds));
                            vorbis.SamplePosition = position + offset;
                        });
                    // Return the clip
                    return ac1;
                }
                case AudioType.WAV:
                case AudioType.UNKNOWN:
                {
                    await UniTask.SwitchToMainThread();
                    Uri.TryCreate(path, UriKind.Absolute, out Uri uri);
                    UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uri, (AudioType)audioType);
                    await uwr.SendWebRequest();
                    if (uwr.error != null) throw new ArgumentException();
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
                    audioClip.name = clipName;
                    return audioClip;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async UniTask<AudioType?> GetAudioTypeFromFile(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] data = new byte[4];
            Array.Clear(data, 0, data.Length);
            int _ = await fileStream.ReadAsync(data, 0, data.Length);
            fileStream.Close();
            if (data.SplitByteArrayToString(3) == "ID3") return AudioType.MPEG;
            if (data.SplitByteArrayToString(4) == "OggS") return AudioType.OGGVORBIS;
            if (data.SplitByteArrayToString(4) == "RIFF") return AudioType.WAV;
            if (data.SplitByteArrayToString(4) == "fLaC") return null;
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

        // public static Sprite ReadSprite(byte[] data, Vector2 pivot, float pixelsPerUnit = 100f)
        // {
        //     return ReadSprite(ReadFileAsTexture(data), pivot, pixelsPerUnit);
        // }

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

            if (!File.Exists(tmpDirPath + "/chara") || !File.Exists(tmpDirPath + "/index.json") ||
                !File.Exists(tmpDirPath + "/hash"))
            {
                onFormatInvalidFound?.Invoke();
                throw new ArgumentException();
            }

            string configStr;
            byte[] textureData = File.ReadAllBytes(tmpDirPath + "/chara");
            byte[] configData = File.ReadAllBytes(tmpDirPath + "/index.json");
            byte[] hashData = File.ReadAllBytes(tmpDirPath + "/hash");
            try
            {
                if (!ValidateFileHash(hashData, textureData, configData)) throw new ArgumentException();
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
                InfoData = configData,
                HashData = hashData,
                Info = externalCharacterInfo
            };
        }

        public static string[] Split(this string str)
        {
            List<string> list = new List<string>();
            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(str);
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.GetTextElement());
            }

            return list.ToArray();
        }

        public static bool ValidateFileHash(byte[] hashData, byte[] imageData, byte[] configData)
        {
            byte[] decrypt = FileEncryptor.RsaDecrypt(hashData);
            if (decrypt.Length != 64) throw new ArgumentException();
            return decrypt.Take(32).ToArray().SequenceEqual(FileEncryptor.ComputeSha256(imageData)) &&
                   decrypt.Skip(32).ToArray().SequenceEqual(FileEncryptor.ComputeSha256(configData));
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

        public static string DataPath => PlayerPrefs.GetString("file_path", GetGameFilePath());

        public static bool Contains(this int[] arr, int i)
        {
            return arr.Any(t => i == t);
        }

        public static async void DisplayException(Exception exception)
        {
            InGameUIManager.ShowModalWindow("错误", exception.Message + "\n" + exception.StackTrace, confirmtext: "确定");
            await UniTask.Delay(1000);
            InGameUIManager.HideModalWindow();
        }

        public static void DisplayNetworkException(string original)
        {
            Debug.LogError(original);
            int lastIndexOf = original.LastIndexOf('{');
            if (lastIndexOf != -1) original = original.Substring(0, lastIndexOf);
            InGameUIManager.ShowModalWindowWithClose("错误", original, () => { }, "确定");
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (FileSystemInfo fi in source.GetFileSystemInfos())
            {
                var targetPath = Path.Combine(target.FullName, fi.Name);

                switch (fi)
                {
                    case FileInfo fileInfo:
                        if (fileInfo.Directory is { Exists: false }) fileInfo.Directory.Create();
                        fileInfo.CopyTo(targetPath, true);
                        break;
                    case DirectoryInfo directoryInfo:
                    {
                        if (!target.Exists) target.Create();
                        DirectoryInfo subDir = target.CreateSubdirectory(directoryInfo.Name);
                        CopyAll(directoryInfo, subDir);
                        break;
                    }
                }
            }
        }

        public static Color ToColor(this string str)
        {
            if (str.StartsWith("#")) str = str[1..];
            else if (str.StartsWith("0x")) str = str[2..];
            if (str.Length != 6 && str.Length != 8)
            {
                throw new ArgumentException();
            }

            byte[] colorByte = { 255, 255, 255, 255 };
            for (int i = 0; i < str.Length / 2; i++)
            {
                colorByte[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            }

            return new Color(colorByte[0] / 255f, colorByte[1] / 255f, colorByte[2] / 255f, colorByte[3] / 255f);
        }

        public static string GetGameFilePath()
        {
            return Application.platform switch
            {
                RuntimePlatform.OSXEditor => Application.persistentDataPath,
                RuntimePlatform.OSXPlayer => Application.persistentDataPath,
                RuntimePlatform.WindowsPlayer => Application.persistentDataPath,
                RuntimePlatform.WindowsEditor => Application.persistentDataPath,
                RuntimePlatform.IPhonePlayer => Application.persistentDataPath,
                RuntimePlatform.LinuxPlayer => Application.persistentDataPath,
                RuntimePlatform.LinuxEditor => Application.persistentDataPath,
                RuntimePlatform.Android => new DirectoryInfo(Application.persistentDataPath + "/../../../../RPGR-Data")
                    .FullName,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}