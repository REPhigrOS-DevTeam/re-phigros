using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Cysharp.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using MainCore.Common;
using MainCore.Data;
using MainCore.UI.Utils;
using MainCore.Utilities;
using Network.Multiplayer.Data;
using Newtonsoft.Json;
using SFB;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utilities;
using YamlDotNet.Serialization;
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

        //private string internalPath = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("/Android"));

        // Start is called before the first frame update
        void Start()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            PlayerPrefs.SetString("file_path", Application.persistentDataPath);
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

        private void RefreshGameFolder()
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

        private void Update()
        {
        }

        public void EnterGame()
        {
            UniTask.Void(EnterGameInternal);
        }

        private async UniTaskVoid EnterGameInternal()
        {
            if (chartLoading || infoDropdown.options.Count == 0)
                return;
            chartLoading = true;
            PlayerPrefs.SetString("chartFolderPath", tempPath);

            GlobalSetting.ChartFolderPath = tempPath;

            SongInfo songInfo;
            GameFilePathInfo pathInfo;
            object obj;
            (songInfo, GlobalSetting.InfoType, pathInfo, obj) = await GameUtils.GetInfoForPlay(tempPath);

            switch (GlobalSetting.InfoType)
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
//#if UNITY_EDITOR || UNITY_STANDALONE_WIN
//            GlobalSetting.chartpath = tempPath + "\\" + chartPathDropdown.captionText.text;
//            GlobalSetting.musicPath = tempPath + "\\" + musicPathDropdown.captionText.text;
//            GlobalSetting.illustrationPath = tempPath + "\\" + illustrationPathDropdown.captionText.text;
//#else
            //tempPath = Path.Combine(internalPath, tempPath.Substring(tempPath.IndexOf("/0") + 2, tempPath.Length));
            // chart settings

//#endif
            await UniTask.SwitchToMainThread();
            PlayerPrefs.Save();
            GlobalSetting.ReadUserSettings();
            await UniTask.SwitchToThreadPool();

            HitSoundManager.Init();

            var extraJsonPath = tempPath + "/extra.json";
            if (File.Exists(extraJsonPath))
            {
                try
                {
                    GlobalSetting.ExtraEvents =
                        JsonConvert.DeserializeObject<Extra>(await File.ReadAllTextAsync(extraJsonPath));
                }
                catch (Exception)
                {
                    GlobalSetting.ExtraEvents = null;
                }
            }
            else
            {
                GlobalSetting.ExtraEvents = null;
            }

            if (GlobalSetting.ExtraEvents is { Videos: { Count: > 0 } })
            {
                await UniTask.SwitchToMainThread();
                InGameUIManager.ShowModalWindowWithClose("警告", "RPGR不支持视频\n<size=10>（其实是不完全支持）（小声）</size>\n除非你愿意捐400美金",
                    () => { }, "确定");
                await UniTask.WaitWhile(() => InGameUIManager.IsActive);
                await UniTask.SwitchToThreadPool();
            }

            //We load chart from here.
            await Main.InitChartAuto(GlobalSetting.ChartPath, false).ConfigureAwait(false);
            if (GlobalSetting.InfoType == InfoType.InfoYml) Main.ApplyPhiraOffset((float)obj);

            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Poyoroid_utsu &&
                (GlobalSetting.Composer.ToLowerInvariant().Contains("pepoyo") ||
                 GlobalSetting.Composer.Contains("ぺぽよ") || GlobalSetting.Composer.Contains("ペポヨ")))
            {
                GlobalSetting.PepoyoDaisuki = GlobalSetting.PepoyoMode.Yande;
            }

            await UniTask.SwitchToMainThread();
            PopupMessageManager.Instance.ChangeContent("loading...");
            await UniTask.SwitchToThreadPool();

            GlobalSetting.IsMultiplayer = false;
            
            (Sprite sprite, Exception exception) = GlobalSetting.IllustrationPath == ""
                ? (Resources.Load<Sprite>("1920x1080_Black"), null)
                : await Util.ReadFileAsSpriteAsync(await File.ReadAllBytesAsync(GlobalSetting.IllustrationPath));
            if (exception != null)
            {
                await UniTask.SwitchToMainThread();
                Debug.LogException(exception);
                InGameUIManager.ShowModalWindowWithClose("读取曲绘文件出错", exception.Message + "\n" + exception.StackTrace,
                    () => { }, "确认");
                return;
            }

            GlobalSetting.BackgroundImage = sprite;

            await UniTask.SwitchToMainThread();
            Main.music = null;
            AudioClip music;
            try
            {
                music = await Util.ReadMusicAsAudioClip(GlobalSetting.MusicPath);
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

            Main.music = music;

            GlobalSetting.UsingApi = false;

            await UniTask.SwitchToMainThread();
            PopupMessageManager.Instance.Clear();
            SceneTransit.Instance.LoadScene("LoadInto");
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
                GlobalSetting.LineImage = new CSVReader(tempPath + "/line.csv");
            }
            else
            {
                GlobalSetting.LineImage = null;
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

        public void SpeedChange()
        {
            speed = float.Parse(GameObject.Find("SpeedDropdown").GetComponent<Dropdown>().captionText.text.Trim('x'));
            GlobalSetting.NoteSpeedFactor = speed;
        }

        public void OnChangeDropdown()
        {
            if (infoDropdown.options.Count == 0) return;
            string t = infoDropdown.captionText.text;

            tempPath = Path.Combine(Util.DataPath /*Application.persistentDataPath*/, t);

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

        public void UnzipPez()
        {
            OpenFile.LoadFile(OnLoadPezSucceeded, () => { }, new[] { new ExtensionFilter("RPE谱包", "pez") }, null,
                "选择Pez...", "确定");
        }

        private void OnLoadPezSucceeded(string zipFile)
        {
            string songFolderName = Path.GetFileNameWithoutExtension(zipFile);
            string destFolderPath = Path.Combine(Util.DataPath, songFolderName);
            if (Directory.Exists(destFolderPath))
            {
                InGameUIManager.ShowModalWindow("提示", $"歌曲“{songFolderName}已存在，确认覆盖？”", () =>
                {
                    Directory.Delete(destFolderPath, true);
                    InGameUIManager.HideModalWindowForcely();
                    Unzip();
                }, "确定", () => { InGameUIManager.HideModalWindow(); }, "取消");
                return;
            }

            Unzip();

            void Unzip()
            {
                ZipUtils.UnZip(zipFile, destFolderPath);
                string externalTextureZip = destFolderPath + "/" + "texture.zip";
                if (File.Exists(externalTextureZip))
                {
                    ZipUtils.UnZip(externalTextureZip, destFolderPath);
                    File.Delete(externalTextureZip);
                }

                InGameUIManager.ShowModalWindowWithClose("提示", "解压成功", () => { }, "确定");
                RefreshGameFolder();
            }
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
}