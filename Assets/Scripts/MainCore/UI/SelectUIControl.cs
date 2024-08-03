/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MainCore.Common;
using MainCore.Data;
using MainCore.UI.Utils;
using MainCore.Utilities;
using Network.Multiplayer.Data;
using Newtonsoft.Json;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UniTask = Cysharp.Threading.Tasks.UniTask;

namespace MainCore.UI
{
    public class SelectUIControl : MonoBehaviour
    {
        public GameObject chartNameUI;
        public Dropdown infoDropdown;
        public Dropdown chartPathDropdown;
        public Dropdown musicPathDropdown;
        public Dropdown illustrationPathDropdown;
        public Button titleButton;

        private int clickCounter;
        private bool chartLoading, otherLoading;
        private float speed;
        private string tempPath;

        // Start is called before the first frame update
        void Start()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            PlayerPrefs.DeleteKey("file_path");
            PlayerPrefs.Save();
#endif

            //PlayerPrefs.SetString("chartFolderPath", tempPath);
            GameObject.Find("DiffInput").GetComponent<InputField>().text =
                PlayerPrefs.GetString("difficultyName", "SP Lv.?");
            chartNameUI.GetComponent<InputField>().text = PlayerPrefs.GetString("chartName", "Untitled");
            RefreshGameFolder();

            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Poyoroid_sou)
            {
                titleButton.onClick.AddListener(() =>
                {
                    clickCounter++;
                    if (clickCounter < 10) return;
                    Debug.Log("郁！");
                    InGameUIManager.ShowModalWindowWithClose("恭喜你发现彩蛋！",
                        "选首曲师含那个p主的曲子Start吧！\n你问是谁？刚刚的确认按钮给了你答案\n支持日文原文和罗马音，罗马音忽略大小写", () => { }, "好耶！");
                    GlobalSetting.PepoyoDaisuki = GlobalSetting.PepoyoMode.Poyoroid_utsu;
                });
            }
            else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.结)
            {
                titleButton.onClick.AddListener(() =>
                {
                    clickCounter++;
                    if (clickCounter < 10) return;
                    Debug.Log("绝冲！");
                    InGameUIManager.ShowModalWindowWithClose("恭喜你发现彩蛋！", "随便选首曲子Start吧！", () => { }, "好耶！");
                    GlobalSetting.YayaKawaii = GlobalSetting.YayaMode.绝冲;
                });
            }
            else
            {
                titleButton.onClick.AddListener(() =>
                {
                    clickCounter++;
                    if (clickCounter < 15) return;
                    SceneTransit.Instance.LoadScene(Random.Range(0, 100) < 50 ? "WahtThe" : "AboutScene");
                });
            }

            GlobalSetting.Reset();
        }

        public void RefreshGameFolder()
        {
            string text = "";
            if (infoDropdown.value != 1)
            {
                text = infoDropdown.captionText.text;
            }

            infoDropdown.ClearOptions();
            infoDropdown.AddOptions(GetFolders(Util.DataPath));
            if (text != "")
            {
                for (var i = 0; i < infoDropdown.options.Count; i++)
                {
                    if (infoDropdown.options[i].text != text) continue;
                    infoDropdown.value = i;
                    break;
                }
            }

            OnChangeDropdown();
        }

        public void EnterGame()
        {
            PopupMessageManager.Instance.Message("Loading...");
            UniTask.Void(EnterGameInternal);
        }

        private async UniTaskVoid EnterGameInternal()
        {
            if (chartLoading || infoDropdown.options.Count == 0)
                return;
            chartLoading = true;
            PlayerPrefs.SetString("chartFolderPath", tempPath);

            GlobalSetting.CurrentBeatmapInfo.BasePath = tempPath;

            SongInfo songInfo;
            GameFilePathInfo pathInfo;
            InfoType infoType;
            object obj;
            (songInfo, infoType, pathInfo, obj) = await GameUtils.GetInfoForPlay(tempPath);

            switch (infoType)
            {
                case InfoType.Empty:
                case InfoType.RpeJson:
                    await UniTask.SwitchToMainThread();
                    GlobalSetting.ChartName = chartNameUI.GetComponent<InputField>().text.Trim();
                    GlobalSetting.Difficulty = GameObject.Find("DiffInput").GetComponent<InputField>().text;
                    await UniTask.SwitchToThreadPool();
                    GlobalSetting.Charter = "Unknown";
                    GlobalSetting.Composer = "Unknown";
                    GlobalSetting.Illustrator = "Unknown";
                    GlobalSetting.ChartPath = Path.Combine(tempPath, chartPathDropdown.captionText.text);
                    GlobalSetting.MusicPath =
                        Path.Combine(tempPath, musicPathDropdown.captionText.text);
                    GlobalSetting.IllustrationPath = illustrationPathDropdown.captionText.text == ""
                        ? ""
                        : Path.Combine(tempPath, illustrationPathDropdown.captionText.text);
                    break;
                case InfoType.InfoTxt:
                case InfoType.InfoCsv:
                case InfoType.InfoCsvOld:
                case InfoType.InfoYml:
                    GlobalSetting.ChartName = songInfo.SongName;
                    GlobalSetting.Difficulty = songInfo.SongDifficulty;
                    GlobalSetting.Charter = songInfo.SongCharter;
                    GlobalSetting.Composer = songInfo.SongComposer;
                    GlobalSetting.Illustrator = songInfo.SongIllustrator;
                    GlobalSetting.ChartPath = Path.Combine(tempPath, pathInfo.Chart);
                    GlobalSetting.MusicPath = Path.Combine(tempPath, pathInfo.Music);
                    GlobalSetting.IllustrationPath = Path.Combine(tempPath, pathInfo.Illustration);
                    break;
                case InfoType.Internal:
                default:
                    break;
            }
            
            await UniTask.SwitchToMainThread();
            PlayerPrefs.Save();
            GlobalSetting.ReadUserSettings();
            await UniTask.SwitchToThreadPool();

            HitSoundManager.UpdateVolume();

            var extraJsonPath = tempPath + "/extra.json";
            if (File.Exists(extraJsonPath))
            {
                try
                {
                    GlobalSetting.CurrentBeatmapInfo.ExtraEvents =
                        JsonConvert.DeserializeObject<Extra>(await File.ReadAllTextAsync(extraJsonPath));
                }
                catch (Exception)
                {
                    GlobalSetting.CurrentBeatmapInfo.ExtraEvents = null;
                }
            }
            else
            {
                GlobalSetting.CurrentBeatmapInfo.ExtraEvents = null;
            }
            
            Debug.Log("Info file loaded");
            //We load chart from here.
            await ChartLoader.InitChartAuto(GlobalSetting.CurrentBeatmapInfo.ChartPath, false).ConfigureAwait(false);
            if (infoType == InfoType.InfoYml) ChartLoader.ApplyPhiraOffset((float)obj);

            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Poyoroid_utsu &&
                (GlobalSetting.CurrentBeatmapInfo.Composer.ToLowerInvariant().Contains("pepoyo") ||
                 GlobalSetting.CurrentBeatmapInfo.Composer.Contains("ぺぽよ") || GlobalSetting.CurrentBeatmapInfo.Composer.Contains("ペポヨ")))
            {
                GlobalSetting.PepoyoDaisuki = GlobalSetting.PepoyoMode.Yande;
            }

            await UniTask.SwitchToMainThread();
            PopupMessageManager.Instance.ChangeContent("Loading...");
            await UniTask.SwitchToThreadPool();

            GlobalSetting.IsMultiplayer = false;

            (Sprite sprite, Exception exception) = GlobalSetting.CurrentBeatmapInfo.IllustrationPath == ""
                ? (Resources.Load<Sprite>("1920x1080_Black"), null)
                : await Util.ReadFileAsSpriteAsync(await File.ReadAllBytesAsync(GlobalSetting.CurrentBeatmapInfo.IllustrationPath));
            if (exception != null)
            {
                await UniTask.SwitchToMainThread();
                Debug.LogException(exception);
                InGameUIManager.ShowModalWindowWithClose("读取曲绘文件出错", exception.Message + "\n" + exception.StackTrace,
                    () => { }, "确认");
                return;
            }

            GlobalSetting.CurrentBeatmapInfo.Illustration = sprite;

            await UniTask.SwitchToMainThread();
            Main.Music = null;
            AudioClip music;
            try
            {
                music = await Util.ReadMusicAsAudioClipAsync(GlobalSetting.CurrentBeatmapInfo.MusicPath);
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

            Main.Music = music;

            await UniTask.SwitchToMainThread();
            PopupMessageManager.Instance.Clear();
            SceneTransit.Instance.LoadScene("LoadingScene");
        }

        public void OnClickPath()
        {
            chartPathDropdown.ClearOptions();
            musicPathDropdown.ClearOptions();
            illustrationPathDropdown.ClearOptions();
            chartPathDropdown.AddOptions(GetFileName(tempPath, ".json", ".pec"));
            musicPathDropdown.AddOptions(GetFileName(tempPath, ".wav", ".ogg", ".mp3"));
            illustrationPathDropdown.AddOptions(GetFileName(tempPath, ".png", ".bmp", ".jpg", ".jpeg"));
            if (File.Exists(tempPath + "/line.csv"))
            {
                GlobalSetting.CurrentBeatmapInfo.LineImage = new CSVReader(tempPath + "/line.csv");
            }
            else
            {
                GlobalSetting.CurrentBeatmapInfo.LineImage = null;
            }
        }

        private static List<Dropdown.OptionData> GetFileName(string path, params string[] typeE) => GameUtils
            .SelectGivenExtensionsFileNames(path, typeE).Select(str => new Dropdown.OptionData(str)).ToList();

        private static List<string> GetFolders(string path)
        {
            List<string> list = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            if (!root.Exists) return list;
            foreach (DirectoryInfo f in root.GetDirectories())
            {
#if UNITY_IPHONE && !UNITY_EDITOR
                if (!f.Name.Trim().StartsWith('.'))
                {
                    list.Add(f.Name.Trim());
                }
#else
                list.Add(f.Name.Trim());
#endif
            }

            list.Sort();

            return list;
        }

        public void OnChangeDropdown()
        {
            if (infoDropdown.options.Count == 0) return;
            string t = infoDropdown.captionText.text;

            tempPath = Path.Combine(Util.DataPath /*Application.persistentDataPath#1#, t);

            if (t.Sum(x => x == '.' ? 1 : 0) >= 2)
            {
                var temp = t.Split('.').ToList().Take(Mathf.Max(t.Split('.').Length - 2, 0));
                chartNameUI.GetComponent<InputField>().text = string.Join(".", temp);
                try
                {
                    GameObject.Find("DiffInput").GetComponent<InputField>().text =
                        $"{t.Split('.')[^2]}  Lv." + t.Split('.')[^1];
                }
                catch
                {
                    GameObject.Find("DiffInput").GetComponent<InputField>().text = "SP  Lv.?";
                }
            }
            else
            {
                chartNameUI.GetComponent<InputField>().text = t;
                GameObject.Find("DiffInput").GetComponent<InputField>().text = "SP  Lv.?";
            }

            OnClickPath();
        }

        public void EnterMain()
        {
            SceneTransit.Instance.Back();
        }

        public void TryUnzipPez()
        {
            OpenFile.LoadFile(zipFile =>
                {
                    GameUtils.UnzipChartArchive(zipFile, RefreshGameFolder, InGameUIManager.ShowModalWindowWithCloseFromWindowInfo);
                }, () => { }, new[] { new ExtensionFilter("RPE谱包", "pez") }, null,
                "选择Pez...", "确定");
        }
        
#if !UNITY_EDITOR
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                RefreshGameFolder();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                RefreshGameFolder();
            }
        }
#endif
    }
}*/