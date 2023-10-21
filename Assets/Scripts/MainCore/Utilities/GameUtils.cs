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
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

namespace MainCore.Utilities
{
    public static class GameUtils
    {
        public static float ScreenDelta => Mathf.Min((float)Screen.width / Screen.height * 0.5625f, 1f);
        public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha)
        {
            Color color = spriteRenderer.color;
            spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
        }

        public static void SetAlpha(this Graphic graphic, float alpha) => graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);

        public static Color SetAlpha(this Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

        public static Vector3 SetZ(this Vector2 pos, float z) => new Vector3(pos.x, pos.y, z);

        public static Vector2 GetTransformedXY(Vector2 xy)
        {
            return new Vector2(xy.x * ScreenDelta, xy.y);
        }

        public static float GetAspectX(float x)
        {
            return x * ScreenDelta;
        }

        public static bool ResetDSPBuffer(int pow)
        {
            var config = AudioSettings.GetConfiguration();
            config.dspBufferSize = (int)Math.Pow(2, pow);
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
            LchzhInfoOld lchzhInfoOld = null;
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
                bool useLchzh = false;
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
                        useLchzh = true;
                    }
                    catch (Exception ex) when (ex is IndexOutOfRangeException or CsvHelper.MissingFieldException)
                    {
                        try
                        {
                            lchzhInfoOld = csvReader.GetRecords<LchzhInfoOld>().Reverse().ToArray()[0];
                            infoType = InfoType.InfoCsvOld;
                            useLchzh = true;
                        }
                        catch (Exception e) when (e is IndexOutOfRangeException or CsvHelper.MissingFieldException)
                        {
                            InGameUIManager.ShowModalWindowWithClose("警告", "无法读取info.csv，可能是旧版格式", () => { }, "确认");
                        }
                        catch (Exception)
                        {
                            InGameUIManager.ShowModalWindowWithClose("错误",
                                "该设备无法读取csv文件\n请联系开发者并提供设备品牌、具体型号、系统名称与版本等信息",
                                () => { }, "确认");
                        }
                    }
                    catch (Exception)
                    {
                        InGameUIManager.ShowModalWindowWithClose("错误", "该设备无法读取csv文件\n请联系开发者并提供设备品牌、具体型号、系统名称与版本等信息",
                            () => { }, "确认");
                    }
                }

                if (!useLchzh)
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
                InfoType.InfoCsvOld => lchzhInfoOld,
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
                    gameFilePathInfo.Chart = infoTxtReader.GetChartFileName();
                    gameFilePathInfo.Music = infoTxtReader.GetSongFileName();
                    gameFilePathInfo.Illustration = infoTxtReader.GetIllustrationFileName();
                    break;
                case InfoType.InfoCsv:
                    LchzhInfo lchzhInfo = obj as LchzhInfo;
                    gameFilePathInfo.Chart = lchzhInfo.Chart;
                    gameFilePathInfo.Music = lchzhInfo.Music;
                    gameFilePathInfo.Illustration = lchzhInfo.Image;
                    break;
                case InfoType.InfoCsvOld:
                    LchzhInfoOld lchzhInfoOld = obj as LchzhInfoOld;
                    gameFilePathInfo.Chart = lchzhInfoOld.Chart;
                    gameFilePathInfo.Music = lchzhInfoOld.Music;
                    gameFilePathInfo.Illustration = lchzhInfoOld.Image;
                    break;
                case InfoType.InfoYml:
                    PhiraInfoData phiraInfoData = obj as PhiraInfoData;
                    gameFilePathInfo.Chart = phiraInfoData.chart;
                    gameFilePathInfo.Music = phiraInfoData.music;
                    gameFilePathInfo.Illustration = phiraInfoData.illustration;
                    break;
                case InfoType.RpeJson:
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
        [JsonProperty("Chart")] public string Chart;
        [JsonProperty("Music")] public string Music;
        [JsonProperty("Illustration")] public string Illustration;
    }
}