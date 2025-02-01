using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using MainCore.Data;
using MainCore.UI;
using MainCore.UI.Utils;
using MainCore.Utilities;
using MainCore.Utilities.ResourceManager;
using Newtonsoft.Json;
using UnityEngine;

namespace MainCore.Serialized
{
    [Serializable]
    public class BeatmapInfo
    {
        public string IllustrationPath { get; set; }
        public string MusicPath { get; set; }
        public string ChartPath { get; set; }
        public string SongName { get; set; } = "Song Name";
        public string SongLevel { get; set; } = "SP  Lv.?";
        public string Composer { get; set; } = "Unknown";
        public string Illustrator { get; set; } = "Unknown";
        public string Charter { get; set; } = "Unknown";

        [NonSerialized] public string BasePath = "";
        [NonSerialized] public Texture2D Illustration = null;
        [NonSerialized] public AudioClip Music;
        [NonSerialized] public string RawChart;
        [NonSerialized] public Extra ExtraEvents = new();
        [NonSerialized] public CSVReader LineImage = null;
        [NonSerialized] public float YmlOffset;
        [NonSerialized] public InfoType InfoType = InfoType.Empty;

        public async UniTask<BeatmapInfo> ReloadFromPathFallback(string path)
        {
            Debug.Log($"[BeatmapInfoLoader] Reloading info for {path}");
            
            var (songInfo, infoType, pathInfo, ymlOffset) = await GameUtils.GetInfoForPlay(path);

            Debug.Log($"[BeatmapInfoLoader] Got info for {path}, was {infoType}");

            if (infoType is InfoType.Empty)
            {
                return null;
            }
            InfoType = infoType;
            if (InfoType is InfoType.InfoYml)
            {
                YmlOffset = (float) ymlOffset;
            }

            IllustrationPath = pathInfo.Illustration;
            MusicPath = pathInfo.Music;
            ChartPath = pathInfo.Chart;
            
            //Sanity Check
            var fileList = new List<string>(){IllustrationPath, MusicPath, ChartPath};
            if (!fileList.TrueForAll(x => File.Exists(Path.Combine(path, x))))
            {
                return null;
            }

            SongName = songInfo.SongName;
            SongLevel = songInfo.SongDifficulty;
            Composer = songInfo.SongComposer;
            Illustrator = songInfo.SongIllustrator;
            Charter = songInfo.SongCharter;

            return this;
        }

        public async UniTask LoadIllustration(bool forceRefresh = false)
        {
            if (Illustration && !forceRefresh)
            {
                return;
            }
            var sprite = IllustrationPath == ""
                ? Resources.Load<Texture2D>("1920x1080_Black")
                : await TextureReader.ReadLocalTextureByPath(Path.Combine(BasePath, IllustrationPath));
            Illustration = sprite;
        }
        
        public async UniTask<bool> LoadBeatmap()
        {
            //Preparation
            GlobalSetting.IsMultiplayer = false;
            await UniTask.SwitchToMainThread();
            PlayerPrefs.Save();
            GlobalSetting.ReadUserSettings();
            await UniTask.SwitchToThreadPool();
            HitSoundManager.UpdateVolume();

            //Load extra.json
            var extraJsonPath = Path.Combine(BasePath, "extra.json");
            if (File.Exists(extraJsonPath))
            {
                try
                {
                    ExtraEvents =
                        JsonConvert.DeserializeObject<Extra>(await File.ReadAllTextAsync(extraJsonPath));
                }
                catch (Exception)
                {
                    ExtraEvents = null;
                }
            }
            else
            {
                ExtraEvents = null;
            }
            
            //Load chart
            if (!File.Exists(Path.Combine(BasePath, ChartPath)))
            {
                return false;
            }
            
            RawChart = await ChartLoader.InitChartAuto(Path.Combine(BasePath, ChartPath), false)!.ConfigureAwait(false);
            ChartLoader.ApplyPhiraOffset(YmlOffset);

            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Poyoroid_utsu &&
                (Composer.ToLowerInvariant().Contains("pepoyo") ||
                 Composer.Contains("ぺぽよ") || Composer.Contains("ペポヨ")))
            {
                GlobalSetting.PepoyoDaisuki = GlobalSetting.PepoyoMode.Yande;
            }

            await UniTask.SwitchToMainThread();
            PopupMessageManager.Instance.ChangeContent("Loading...");
            await UniTask.Delay(500);
            await UniTask.SwitchToThreadPool();
            
            //Load illustration
            await LoadIllustration();
            
            await UniTask.SwitchToMainThread();
            
            //Load music
            AudioClip music;
            try
            {
                music = await Util.ReadMusicAsAudioClipAsync(Path.Combine(BasePath, MusicPath));
            }
            catch (Exception e)
            {
                await UniTask.SwitchToMainThread();
                Debug.LogException(e);
                InGameUIManager.ShowModalWindowWithClose("读取音频文件出错", e.Message, () => { },
                    "确认");
                return false;
            }
            if (!music)
            {
                await UniTask.SwitchToMainThread();
                Debug.Log("[BeatmapInfoLoader] 不支持的音频格式");
                InGameUIManager.ShowModalWindowWithClose("错误", "检测到音频文件为不支持的格式",
                    () => { PopupMessageManager.Instance.ChangeContent(""); }, "确认");
                return false;
            }
            Music = music;

            await UniTask.SwitchToMainThread();
            return true;
        }
    }
}