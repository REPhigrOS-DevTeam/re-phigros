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

        public static void SetAlpha(this Graphic graphic, float alpha) =>
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);

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
            if (!GlobalSetting.GameStarted || events.Count == 0)
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
            if (!GlobalSetting.GameStarted || events.Count == 0)
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
            if (!GlobalSetting.GameStarted || events.Count == 0)
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
            if (!GlobalSetting.GameStarted || events.Count == 0)
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
            PhiraChartInfoData phiraChartInfoData = null;
            LchzhInfo lchzhInfo = null;
            LchzhInfoOld lchzhInfoOld = null;
            InfoTxtReader infoTxtReader = null;
            RpeChartData.RpeMeta rpeMeta = null;
            if (File.Exists(phiraInfoPath))
            {
                IDeserializer deserializer = new DeserializerBuilder().Build();
                phiraChartInfoData =
                    deserializer.Deserialize<PhiraChartInfoData>(await File.ReadAllTextAsync(phiraInfoPath));
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
                            InGameUIManager.ShowModalWindowWithClose("警告", "无法读取info.csv，可能是其他旧版格式", () => { }, "确认");
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
                    string[] jsons = Directory.GetFiles(directory, "*.json")
                        .Where(str => Path.GetFileName(str).ToLowerInvariant() != "extra.json").ToArray();
                    if (jsons.Length > 0)
                    {
                        string ch = await File.ReadAllTextAsync(jsons[0]);
                        if (!ch.Contains("formatVersion") && ch.Contains("}") && ch.Contains("numOfNotes"))
                        {
                            rpeMeta = JsonUtility.FromJson<RpeChartData>(ch).META;
                            infoType = InfoType.RpeJson;
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
            }

            string musicPath = directory + "/" +
                               (phiraChartInfoData != null && !string.IsNullOrEmpty(phiraChartInfoData.music)
                                   ? phiraChartInfoData.music
                                   : lchzhInfo != null
                                       ? lchzhInfo.Music
                                       : rpeMeta != null
                                           ? rpeMeta.song
                                           : infoTxtReader != null
                                               ? infoTxtReader.GetSongFileName()
                                               : Path.GetFileName(Directory.GetFiles(directory)
                                                   .Where(s => new List<string> { ".wav", ".ogg", ".mp3" }.Contains(
                                                       Path.GetExtension(s).ToLowerInvariant())).ToArray()[0]));

            float? musicLength = (await Util.ReadMusicAsAudioClip(musicPath))?.length;
            return (new SongInfo
            {
                FolderName = Path.GetFileName(directory),
                SongName = phiraChartInfoData != null ? phiraChartInfoData.name :
                    lchzhInfo != null ? lchzhInfo.Name :
                    rpeMeta != null ? rpeMeta.name :
                    infoTxtReader != null ? infoTxtReader.GetName() : Path.GetFileNameWithoutExtension(Directory
                        .GetFiles(directory)
                        .Where(s => new List<string> { ".wav", ".ogg", ".mp3" }.Contains(
                            Path.GetExtension(s).ToLowerInvariant())).ToArray()[0]),
                SongComposer = phiraChartInfoData != null ? phiraChartInfoData.composer :
                    lchzhInfo != null ? lchzhInfo.Artist :
                    rpeMeta != null ? rpeMeta.composer :
                    infoTxtReader != null ? infoTxtReader.GetComposer() : "Unknown",
                SongDifficulty = phiraChartInfoData != null ? phiraChartInfoData.level :
                    lchzhInfo != null ? lchzhInfo.Level :
                    rpeMeta != null ? rpeMeta.level :
                    infoTxtReader != null ? infoTxtReader.GetDifficulty() : "SP  Lv.?",
                SongCharter = phiraChartInfoData != null ? phiraChartInfoData.charter :
                    lchzhInfo != null ? lchzhInfo.Charter :
                    rpeMeta != null ? rpeMeta.charter :
                    infoTxtReader != null ? infoTxtReader.GetCharter() : "Unknown",
                SongIllustrator = phiraChartInfoData != null ? phiraChartInfoData.illustrator :
                    lchzhInfo != null ? lchzhInfo.Illustrator : "Unknown",
                MusicLength = musicLength ?? -1f
            }, infoType, infoType switch
            {
                InfoType.Empty => null,
                InfoType.InfoTxt => infoTxtReader,
                InfoType.InfoCsv => lchzhInfo,
                InfoType.InfoCsvOld => lchzhInfoOld,
                InfoType.InfoYml => phiraChartInfoData,
                InfoType.RpeJson => rpeMeta,
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
                    PhiraChartInfoData phiraChartInfoData = obj as PhiraChartInfoData;
                    gameFilePathInfo.Chart = phiraChartInfoData.chart;
                    gameFilePathInfo.Music = phiraChartInfoData.music;
                    gameFilePathInfo.Illustration = phiraChartInfoData.illustration;
                    break;
                case InfoType.RpeJson:
                    RpeChartData.RpeMeta rpeMeta = obj as RpeChartData.RpeMeta;
                    gameFilePathInfo.Chart = Directory.GetFiles(directory, "*.json").Select(Path.GetFileName).Where(str => str.ToLowerInvariant() != "extra.json").ToArray()[0];
                    gameFilePathInfo.Music = rpeMeta.song;
                    gameFilePathInfo.Illustration = rpeMeta.background;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (songInfo, infoType, gameFilePathInfo,
                infoType == InfoType.InfoYml ? ((PhiraChartInfoData)obj).offset : null);
        }

        public static string[] SelectGivenExtensionsFileNames(string directory, params string[] extensions) => Directory
            .GetFiles(directory).Where(s => extensions.Select(str => str.ToLowerInvariant()).ToList()
                .Contains(Path.GetExtension(s).ToLowerInvariant()))
            .Select(Path.GetFileName).ToArray();

        private static string[] constantSkinFile =
        {
            "info.yml", "click.png", "click_mh.png", "drag.png", "drag_mh.png", "flick.png", "flick_mh.png", "hold.png",
            "hold_mh.png", "hit_fx.png"
        };

        public static async UniTask<SkinInfo> ReadSkin(string dirPath)
        {
            List<string> files = Directory.GetFiles(dirPath).Select(Path.GetFileName).ToList();
            if (constantSkinFile.Any(s => !files.Contains(s)))
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "资源包不完整", () => { }, "确定");
                return null;
            }

            PhiraSkinInfoData phiraSkinInfoData;
            try
            {
                IDeserializer deserializer = new DeserializerBuilder().Build();
                phiraSkinInfoData =
                    deserializer.Deserialize<PhiraSkinInfoData>(await File.ReadAllTextAsync($"{dirPath}/info.yml"));
            }
            catch (IOException)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "无法读取文件", () => { }, "确定");
                return null;
            }
            catch (Exception e)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "info.yml格式不正确", () => { }, "确定");
                Debug.LogException(e);
                return null;
            }

            if (phiraSkinInfoData.name == null || phiraSkinInfoData.author == null ||
                phiraSkinInfoData.hitFx == null || phiraSkinInfoData.holdAtlas == null ||
                phiraSkinInfoData.holdAtlasMH == null || phiraSkinInfoData.hitFx.Length != 2 ||
                phiraSkinInfoData.holdAtlas.Length != 2 || phiraSkinInfoData.holdAtlasMH.Length != 2)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "info.yml不完整", () => { }, "确定");
                return null;
            }

            SkinInfo skinInfo = ScriptableObject.CreateInstance<SkinInfo>();
            skinInfo.name = skinInfo.skinName = phiraSkinInfoData.name;
            skinInfo.author = phiraSkinInfoData.author;
            skinInfo.description = phiraSkinInfoData.description == "" ? "无" : phiraSkinInfoData.description;
            skinInfo.hitFxDuration = phiraSkinInfoData.hitFxDuration;
            skinInfo.hitFxScale = phiraSkinInfoData.hitFxScale;
            skinInfo.hitFxRotate = phiraSkinInfoData.hitFxRotate;
            skinInfo.hitFxTinted = phiraSkinInfoData.hitFxTinted;
            skinInfo.hideParticles = phiraSkinInfoData.hideParticles;
            skinInfo.holdKeepHead = phiraSkinInfoData.holdKeepHead;
            // skinInfo.holdRepeat = phiraSkinInfoData.holdRepeat;
            skinInfo.holdRepeat = false;
            skinInfo.holdCompact = phiraSkinInfoData.holdCompact;
            if (phiraSkinInfoData.colorPerfect.ToLowerInvariant() == "0xe1ffec9f")
                phiraSkinInfoData.colorPerfect = "0xfffeffad";
            if (phiraSkinInfoData.colorGood.ToLowerInvariant() == "0xebb4e1ff")
                phiraSkinInfoData.colorGood = "0xff8cecff";
            if (phiraSkinInfoData.colorGood.ToLowerInvariant() == "0xe1ffec9f")
                phiraSkinInfoData.colorGood = "0xfffeffad";
            if (phiraSkinInfoData.colorPerfect.ToLowerInvariant() == "0xebb4e1ff")
                phiraSkinInfoData.colorPerfect = "0xff8cecff";
            skinInfo.perfectColor = phiraSkinInfoData.colorPerfect.ARGBToColor();
            skinInfo.goodColor = phiraSkinInfoData.colorGood.ARGBToColor();
            // 读取Hit Fx
            List<Sprite> hitFx = new List<Sprite>();
            int numColumns = phiraSkinInfoData.hitFx[0];
            int numRows = phiraSkinInfoData.hitFx[1];
            Texture2D hitFxTexture = Util.ReadFileAsTexture(await File.ReadAllBytesAsync($"{dirPath}/hit_fx.png"));
            if (hitFxTexture.width % numColumns != 0 || hitFxTexture.height % numRows != 0)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "打击特效图片长宽与info.yml的内容不匹配", () => { }, "确定");
                return null;
            }

            Vector2 spriteSize =
                new Vector2(hitFxTexture.width / numColumns, hitFxTexture.height / numRows); // 按列和行切割图片

            for (int y = 0; y < numRows; ++y)
            {
                for (int x = 0; x < numColumns; ++x)
                {
                    Rect rect = new Rect(x * spriteSize.x, (numRows - y - 1) * spriteSize.y, spriteSize.x,
                        spriteSize.y);

                    Sprite sprite =
                        Sprite.Create(hitFxTexture, rect, new Vector2(0.5f, 0.5f), spriteSize.x / 2.5f,
                            1); // 创建新的 Sprite 对象
                    sprite.name = $"hit_fx_external_{y * numColumns + x}"; // 自定义 Sprite 的名字

                    hitFx.Add(sprite);
                }
            }

            skinInfo.hitFx = hitFx.ToArray();
            // 读取note们
            (skinInfo.click_bad, _) = (skinInfo.click, _) = await Util.ReadFileAsSpriteAsync(await File.ReadAllBytesAsync($"{dirPath}/click.png"));
            (skinInfo.clickMh, _) = await Util.ReadFileAsSpriteAsync(await File.ReadAllBytesAsync($"{dirPath}/click_mh.png"));
            Texture2D dragTex = await Util.ReadFileAsTextureAsync(await File.ReadAllBytesAsync($"{dirPath}/drag.png"));
            skinInfo.drag = Sprite.Create(dragTex, new Rect(0, 0, dragTex.width, dragTex.height),
                new Vector2(0.5f, 0.5f), skinInfo.click.pixelsPerUnit * dragTex.width / skinInfo.click.rect.width, 1);
            Texture2D dragMhTex = await Util.ReadFileAsTextureAsync(await File.ReadAllBytesAsync($"{dirPath}/drag_mh.png"));
            skinInfo.dragMh = Sprite.Create(dragMhTex, new Rect(0, 0, dragMhTex.width, dragMhTex.height),
                new Vector2(0.5f, 0.5f), skinInfo.clickMh.pixelsPerUnit * dragMhTex.width / skinInfo.clickMh.rect.width,
                1);
            Texture2D flickTex = await Util.ReadFileAsTextureAsync(await File.ReadAllBytesAsync($"{dirPath}/flick.png"));
            skinInfo.flick = Sprite.Create(flickTex, new Rect(0, 0, flickTex.width, flickTex.height),
                new Vector2(0.5f, 0.5f), skinInfo.click.pixelsPerUnit * flickTex.width / skinInfo.click.rect.width, 1);
            Texture2D flickMhTex = await Util.ReadFileAsTextureAsync(await File.ReadAllBytesAsync($"{dirPath}/flick_mh.png"));
            skinInfo.flickMh = Sprite.Create(flickMhTex, new Rect(0, 0, flickMhTex.width, flickMhTex.height),
                new Vector2(0.5f, 0.5f),
                skinInfo.clickMh.pixelsPerUnit * flickMhTex.width / skinInfo.clickMh.rect.width,
                1);
            try
            {
                // hold
                Texture2D holdTexture = await Util.ReadFileAsTextureAsync(await File.ReadAllBytesAsync($"{dirPath}/hold.png"));
                Sprite[] holdSprites = SplitTexture("hold", holdTexture, phiraSkinInfoData.holdAtlas[0],
                    phiraSkinInfoData.holdAtlas[1], skinInfo.click.pixelsPerUnit, skinInfo.click.rect.width);
                skinInfo.holdHead = holdSprites[0];
                skinInfo.holdBody = holdSprites[1];
                skinInfo.holdEnd = holdSprites[2];
                skinInfo.holdLengthFactor = skinInfo.holdBody.rect.height / skinInfo.holdBody.pixelsPerUnit;
                Texture2D holdMhTexture =
                    await Util.ReadFileAsTextureAsync(await File.ReadAllBytesAsync($"{dirPath}/hold_mh.png"));
                Sprite[] holdMhSprites = SplitTexture("hold_mh", holdMhTexture, phiraSkinInfoData.holdAtlasMH[0],
                    phiraSkinInfoData.holdAtlasMH[1], skinInfo.clickMh.pixelsPerUnit, skinInfo.clickMh.rect.width);
                skinInfo.holdHeadMh = holdMhSprites[0];
                skinInfo.holdBodyMh = holdMhSprites[1];
                skinInfo.holdEndMh = holdMhSprites[2];
                skinInfo.hitParticle = SkinManager.Instance.defaultParticle;
                skinInfo.holdMhLengthFactor = skinInfo.holdBodyMh.rect.height / skinInfo.holdBodyMh.pixelsPerUnit;
            }
            catch (ArgumentException)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "Hold贴图高度不足", () => { }, "确定");
                return null;
            }

            // BUG: 无法正常播放打击音
            skinInfo.clickAC = false && File.Exists($"{dirPath}/click.ogg")
                ? await Util.ReadMusicAsAudioClip($"{dirPath}/click.ogg", "click", true)
                : SkinManager.Instance.defaultClickAC;
            skinInfo.dragAC = false && File.Exists($"{dirPath}/drag.ogg")
                ? await Util.ReadMusicAsAudioClip($"{dirPath}/drag.ogg", "drag", true)
                : SkinManager.Instance.defaultDragAC;
            skinInfo.flickAC = false && File.Exists($"{dirPath}/flick.ogg")
                ? await Util.ReadMusicAsAudioClip($"{dirPath}/flick.ogg", "flick", true)
                : SkinManager.Instance.defaultFlickAC;

            return skinInfo;
        }

        private static Sprite[] SplitTexture(string namePrefix, Texture2D texture, int startPixel, int endPixel,
            float clickPpu,
            float clickWidth)
        {
            if (startPixel + endPixel > texture.height)
            {
                throw new ArgumentException();
            }

            float ppu = clickPpu * texture.width / clickWidth;
            Sprite head = Sprite.Create(texture, new Rect(0f, 0f, texture.width, startPixel), new Vector2(0.5f, 1f),
                ppu, 1);
            Sprite body = Sprite.Create(texture,
                new Rect(0f, startPixel, texture.width, texture.height - startPixel - endPixel), new Vector2(0.5f, 1f),
                ppu, 1);
            Sprite end = Sprite.Create(texture, new Rect(0f, texture.height - endPixel, texture.width, endPixel),
                new Vector2(0.5f, 0f), ppu, 1);
            head.name = $"{namePrefix}_head";
            body.name = $"{namePrefix}_body";
            end.name = $"{namePrefix}_end";
            return new[] { head, body, end };
        }
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