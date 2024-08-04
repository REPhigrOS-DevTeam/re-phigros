using System;
using System.IO;
using Cysharp.Threading.Tasks;
using MainCore.Data;
using MainCore.UI;
using MainCore.UI.Utils;
using MainCore.Utilities;
using Newtonsoft.Json;
using Unity.Assertions;
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
        [NonSerialized] public Sprite Illustration;
        [NonSerialized] public AudioClip Music;
        [NonSerialized] public string RawChart;
        [NonSerialized] public Extra ExtraEvents = new();
        [NonSerialized] public CSVReader LineImage = null;
        [NonSerialized] public float YmlOffset;
        [NonSerialized] public InfoType InfoType = InfoType.Empty;

        public async UniTask<BeatmapInfo> ReloadFromPathFallback(string path)
        {
            Debug.Log($"[Fallback] Loading info for {path}");
            
            var (songInfo, infoType, pathInfo, ymlOffset) = await GameUtils.GetInfoForPlay(path);

            Debug.Log($"Got info for {path}, was {infoType}");

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

            SongName = songInfo.SongName;
            SongLevel = songInfo.SongDifficulty;
            Composer = songInfo.SongComposer;
            Illustrator = songInfo.SongIllustrator;
            Charter = songInfo.SongCharter;

            return this;
        }

        public async UniTask LoadIllustration(bool forceRefresh = false)
        {
            if (!Illustration && !forceRefresh)
            {
                return;
            }
            var (sprite, exception) = IllustrationPath == ""
                ? (Resources.Load<Sprite>("1920x1080_Black"), null)
                : await Util.ReadFileAsSpriteAsync(await File.ReadAllBytesAsync(Path.Combine(BasePath, IllustrationPath)));
            if (exception != null)
            {
                //Should never run to here...
                Assert.IsTrue(false);
                await UniTask.SwitchToMainThread();
                Debug.LogException(exception);
                InGameUIManager.ShowModalWindowWithClose("读取曲绘文件出错", $"读取{SongName}的曲绘失败：\n不对啊不应该执行到这里啊\n" + exception.Message,
                    () => { }, "确认");
                return;
            }
            Illustration = sprite;
        }
        
        public async UniTask LoadBeatmap()
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
                InGameUIManager.ShowModalWindowWithClose("读取音频文件出错", e.Message + "\n" + e.StackTrace, () => { },
                    "确认");
                return;
            }
            if (!music)
            {
                await UniTask.SwitchToMainThread();
                Debug.Log("不支持的flac格式");
                InGameUIManager.ShowModalWindowWithClose("错误", "检测到音频文件为不支持的flac格式",
                    () => { PopupMessageManager.Instance.ChangeContent(""); }, "确认");
                return;
            }
            Music = music;

            await UniTask.SwitchToMainThread();
            PopupMessageManager.Instance.Clear();
        }
    }
}