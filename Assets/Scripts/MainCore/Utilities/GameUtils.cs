using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Cysharp.Threading.Tasks;
using MainCore.Data;
using Network.Multiplayer.Data;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MainCore.Utilities
{
    public static class GameUtils
    {
        private static float _screenDelta = -10;

        public static float ScreenDelta
        {
            get
            {
#if UNITY_EDITOR
                _screenDelta = Mathf.Min((float)Screen.width / Screen.height * 0.5625f, 1f);
#else
                if (_screenDelta < 0)
                    _screenDelta = Mathf.Min((float)Screen.width / Screen.height * 0.5625f, 1f);
#endif
                return _screenDelta;
            }
        }

        public static Color SetAlpha(this Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

        public static Vector3 SetZ(this Vector2 pos, float z) => new Vector3(pos.x, pos.y, z);

        public static Vector2 GetTransformedXY(Vector2 xy)
        {
            return new Vector2(xy.x * _screenDelta, xy.y);
        }

        public static float GetAspectX(float x)
        {
            return x * _screenDelta;
        }

        public static bool ResetDSPBuffer(float pow)
        {
            var config = AudioSettings.GetConfiguration();
            config.dspBufferSize = (int)Math.Pow(2, (int)pow);
            return AudioSettings.Reset(config);
        }

        public static void AddTestCount()
        {
#if UNITY_EDITOR
            Main.Mian.TEST_COUNT++;
#endif
        }

        public static void Print(this Exception exception)
        {
            Debug.LogException(exception);
        }

        #region TEMPPPP

        public static judgeLineEvent GetEventFromCurrentTime(List<judgeLineEvent> events, float time)
        {
            if (!GlobalSetting.Playing || events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].startTime < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].startTime >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].startTime >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].startTime < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        public static judgeLineColorEvent GetEventFromCurrentTime(List<judgeLineColorEvent> events, float time)
        {
            if (!GlobalSetting.Playing || events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].startTime < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].startTime >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].startTime >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].startTime < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        public static judgeLineTextEvent GetEventFromCurrentTime(List<judgeLineTextEvent> events, float time)
        {
            if (!GlobalSetting.Playing || events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].startTime < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].startTime >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].startTime >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].startTime < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        public static judgeLineSpeedEvent GetEventFromCurrentTime(List<judgeLineSpeedEvent> events, float time)
        {
            if (events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].startTime < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].startTime >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].startTime >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].startTime < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        public static noteControl GetEventFromCurrentTime(List<noteControl> events, float time)
        {
            if (!GlobalSetting.Playing || events.Count == 0)
                return null;

            //Binary search
            int l, r, length;
            length = events.Count - 1;
            l = 0;
            r = length;
            var tempIndex = 0;

            while (l <= r)
            {
                int mid = (l + r) / 2;
                if (events[mid].start < time)
                {
                    if (mid < length)
                    {
                        if (events[mid + 1].start >= time)
                        {
                            tempIndex = mid;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    l = mid + 1;
                }
                else if (events[mid].start >= time)
                {
                    if (mid >= 1)
                    {
                        if (events[mid - 1].start < time)
                        {
                            tempIndex = mid - 1;
                            break;
                        }
                    }
                    else
                    {
                        tempIndex = mid;
                        break;
                    }

                    r = mid - 1;
                }
            }

            return events[tempIndex];
        }

        #endregion

        public static async UniTask<SongInfo> GetSongInfo(string directory)
        {
            string phiraInfoPath = directory + "/info.yml";
            PhiraInfoData phiraInfoData = null;
            if (File.Exists(phiraInfoPath))
            {
                IDeserializer deserializer = new DeserializerBuilder().Build();
                phiraInfoData = deserializer.Deserialize<PhiraInfoData>(await File.ReadAllTextAsync(phiraInfoPath));
            }

            string lchzhInfoPath = directory + "/info.csv";
            LchzhInfo lchzhInfo = null;
            if (File.Exists(lchzhInfoPath))
            {
                CsvReader csvReader = new CsvReader(new StreamReader(lchzhInfoPath),
                    new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = false
                    });
                try
                {
                    lchzhInfo = csvReader.GetRecords<LchzhInfo>().Reverse().ToArray()[0];
                }
                catch (Exception e) when (e is IndexOutOfRangeException or CsvHelper.MissingFieldException)
                {
                    InGameUIManager.ShowModalWindowWithClose("警告", "无法读取info.csv，可能是旧版格式", () => { }, "确认");
                }
            }

            InfoTxtReader infoTxtReader = null;
            // info init
            if (phiraInfoData == null)
            {
                var infoPath = directory + "/info.txt";
                if (File.Exists(infoPath))
                {
                    infoTxtReader = new InfoTxtReader(infoPath);
                }
            }

            string musicPath = directory + "/" +
                               (phiraInfoData != null && !string.IsNullOrEmpty(phiraInfoData.music)
                                   ? phiraInfoData.music
                                   : lchzhInfo != null
                                       ? lchzhInfo.Music
                                       : infoTxtReader != null
                                           ? infoTxtReader.GetSongFileName()
                                           : Path.GetFileName(Directory.GetFiles(directory)
                                               .Where(s => new List<string> { ".wav", ".ogg", ".mp3" }.Contains(
                                                   Path.GetExtension(s).ToLowerInvariant())).ToArray()[0]));

            float musicLength = (await Util.ReadMusicAsAudioClip(musicPath)).length;
            return new SongInfo
            {
                FolderName = Path.GetFileName(directory),
                SongName = phiraInfoData != null ? phiraInfoData.name :
                    lchzhInfo != null ? lchzhInfo.Name :
                    infoTxtReader != null ? infoTxtReader.GetName() : Path.GetFileNameWithoutExtension(Directory
                        .GetFiles(directory)
                        .Where(s => new List<string> { ".wav", ".ogg", ".mp3" }.Contains(
                            Path.GetExtension(s).ToLowerInvariant())).ToArray()[0]),
                SongComposer = phiraInfoData != null ? phiraInfoData.composer :
                    lchzhInfo != null ? lchzhInfo.Artist :
                    infoTxtReader != null ? infoTxtReader.GetComposer() : "Unknown",
                SongDifficulty = phiraInfoData != null ? phiraInfoData.level :
                    lchzhInfo != null ? lchzhInfo.Level :
                    infoTxtReader != null ? infoTxtReader.GetDifficulty() : "SP  Lv.?",
                SongCharter = phiraInfoData != null ? phiraInfoData.charter :
                    lchzhInfo != null ? lchzhInfo.Charter :
                    infoTxtReader != null ? infoTxtReader.GetCharter() : "Unknown",
                SongIllustrator = phiraInfoData != null ? phiraInfoData.illustrator :
                    lchzhInfo != null ? lchzhInfo.Illustrator : "Unknown",
                MusicLength = musicLength
            };
        }

        public static async UniTask<(InfoType, object)> GetInfo(string directory)
        {
            string phiraInfoPath = Path.Combine(directory, "info.yml");
            PhiraInfoData phiraInfoData;
            if (File.Exists(phiraInfoPath))
            {
                IDeserializer deserializer = new DeserializerBuilder().Build();
                phiraInfoData = deserializer.Deserialize<PhiraInfoData>(await File.ReadAllTextAsync(phiraInfoPath));
                return (InfoType.InfoYml, phiraInfoData);
            }

            string lchzhInfoPath = Path.Combine(directory, "info.csv");
            LchzhInfo lchzhInfo;
            if (File.Exists(lchzhInfoPath))
            {
                try
                {
                    CsvReader csvReader = new CsvReader(new StreamReader(directory),
                        new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = false
                        });
                    lchzhInfo = csvReader.GetRecords<LchzhInfo>().Reverse().ToArray()[0];
                    csvReader.Dispose();
                    return (InfoType.InfoCsv, lchzhInfo);
                }
                catch (Exception ex) when (ex is IndexOutOfRangeException or CsvHelper.MissingFieldException)
                {
                    InGameUIManager.ShowModalWindowWithClose("警告", "无法读取info.csv，可能是旧版格式", () => { }, "确认");
                }
            }

            var infoPath = Path.Combine(directory, "info.txt");
            if (File.Exists(infoPath))
            {
                // GlobalSetting.infoTxt = new InfoTxtReader(infoPath);
                return (InfoType.InfoTxt, new InfoTxtReader(infoPath));
            }

            return (InfoType.Empty, null);
        }
    }

    public class LchzhInfo
    {
        [CsvHelper.Configuration.Attributes.Name("Chart")]
        public string Chart { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Music")]
        public string Music { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Image")]
        public string Image { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Name")]
        public string Name { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Artist")]
        public string Artist { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Level")]
        public string Level { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Illustrator")]
        public string Illustrator { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Charter")]
        public string Charter { get; set; }

        [CsvHelper.Configuration.Attributes.Name("AspectRatio")]
        public double? AspectRatio { get; set; } = 16f / 9f;

        [CsvHelper.Configuration.Attributes.Name("NoteScale")]
        public double? NoteScale { get; set; } = 1f;

        [CsvHelper.Configuration.Attributes.Name("GlobalAlpha")]
        public double? GlobalAlpha { get; set; } = 0.6f;
    }
}