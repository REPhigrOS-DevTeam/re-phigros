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

        public static async UniTask<(SongInfo, InfoType, object)> GetSongInfo(string directory)
        {
            InfoType infoType = InfoType.Empty;
            string phiraInfoPath = directory + "/info.yml";
            PhiraInfoData phiraInfoData = null;
            LchzhInfo lchzhInfo = null;
            InfoTxtReader infoTxtReader = null;
            if (File.Exists(phiraInfoPath))
            {
                IDeserializer deserializer = new DeserializerBuilder().Build();
                phiraInfoData = deserializer.Deserialize<PhiraInfoData>(await File.ReadAllTextAsync(phiraInfoPath));
                infoType = InfoType.InfoYml;
            }
            else
            {
                string lchzhInfoPath = directory + "/info.csv";
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
                        infoType = InfoType.InfoCsv;
                    }
                    catch (Exception e) when (e is IndexOutOfRangeException or CsvHelper.MissingFieldException)
                    {
                        InGameUIManager.ShowModalWindowWithClose("警告", "无法读取info.csv，可能是旧版格式", () => { }, "确认");
                    }
                    catch (Exception)
                    {
                        InGameUIManager.ShowModalWindowWithClose("错误", "该设备无法读取csv文件\n请联系开发者并提供设备品牌、具体型号、系统名称与版本等信息",
                            () => { }, "确认");
                    }
                }
                else
                {
                    // info init
                    var infoPath = directory + "/info.txt";
                    if (File.Exists(infoPath))
                    {
                        infoTxtReader = new InfoTxtReader(infoPath);
                        infoType = InfoType.InfoTxt;
                    }
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
            return (new SongInfo
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
            }, infoType, infoType switch
            {
                InfoType.Empty => null,
                InfoType.InfoTxt => infoTxtReader,
                InfoType.InfoCsv => lchzhInfo,
                InfoType.InfoYml => phiraInfoData,
                _ => throw new ArgumentOutOfRangeException()
            });
        }

        public static async UniTask<(SongInfo, InfoType infoType, GameFilePathInfo, object)> GetInfoForPlay(
            string directory)
        {
            (SongInfo songInfo, InfoType infoType, object obj) = await GetSongInfo(directory);
            GameFilePathInfo gameFilePathInfo = new GameFilePathInfo();
            switch (infoType)
            {
                case InfoType.Empty:
                    gameFilePathInfo = null;
                    break;
                case InfoType.InfoTxt:
                    InfoTxtReader infoTxtReader = obj as InfoTxtReader;
                    gameFilePathInfo.chart = infoTxtReader.GetChartFileName();
                    gameFilePathInfo.music = infoTxtReader.GetSongFileName();
                    gameFilePathInfo.illustration = infoTxtReader.GetIllustrationFileName();
                    break;
                case InfoType.InfoCsv:
                    LchzhInfo lchzhInfo = obj as LchzhInfo;
                    gameFilePathInfo.chart = lchzhInfo.Chart;
                    gameFilePathInfo.music = lchzhInfo.Music;
                    gameFilePathInfo.illustration = lchzhInfo.Image;
                    break;
                case InfoType.InfoYml:
                    PhiraInfoData phiraInfoData = obj as PhiraInfoData;
                    gameFilePathInfo.chart = phiraInfoData.chart;
                    gameFilePathInfo.music = phiraInfoData.music;
                    gameFilePathInfo.illustration = phiraInfoData.illustration;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (songInfo, infoType, gameFilePathInfo,
                infoType == InfoType.InfoYml ? ((PhiraInfoData)obj).offset : null); // TODO
        }

        public static string[] SelectGivenExtensionsFileNames(string directory, params string[] extensions) => Directory
            .GetFiles(directory).Where(s => extensions.Select(str => str.ToLowerInvariant()).ToList()
                .Contains(Path.GetExtension(s).ToLowerInvariant()))
            .Select(Path.GetFileName).ToArray();
#if false
        public static string[] SelectGivenExtensionsFileNames1(string directory, params string[] extensions)
        {
            var files = Directory.GetFiles(directory);
            var result = new List<string>();

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file).ToLowerInvariant();

                foreach (var ext in extensions)
                {
                    if (ext.ToLowerInvariant() != extension) continue;
                    result.Add(Path.GetFileName(file));
                    break;
                }
            }

            return result.ToArray();
        }
#endif
    }

    public class GameFilePathInfo
    {
        public string chart;
        public string music;
        public string illustration;
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
        public string AspectRatio { get; set; } = 16f / 9f + "";

        [CsvHelper.Configuration.Attributes.Name("NoteScale")]
        public string NoteScale { get; set; } = "1.0";

        [CsvHelper.Configuration.Attributes.Name("GlobalAlpha")]
        public string GlobalAlpha { get; set; } = "0.6";
    }
}