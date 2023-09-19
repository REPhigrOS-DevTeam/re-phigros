using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using MainCore.Common;
using MainCore.Data;
using MainCore.Utilities;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utilities;
using YamlDotNet.Serialization;
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
        private bool loaded;
        private bool loading;
        private PhiraInfoData phiraInfoData;
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
                    SceneTransit.Instance.JumpScene("WahtThe");
                });
            }
        }

        private void RefreshGameFolder()
        {
            infoDropdown.ClearOptions();
            infoDropdown.AddOptions(GetFolders(PlayerPrefs.GetString("file_path")));
            OnChangeDropdown();
        }

        private void Update()
        {
            if (!loaded) return;
            if (!loading) return;

            GlobalSetting.isMultiplayer = false;

            loading = false;
            Sprite sprite = Util.ReadFileAsSprite(File.ReadAllBytes(GlobalSetting.illustrationPath), out Exception exception);
            if (exception != null)
            {
                InGameUIManager.ShowModalWindowWithClose("读取曲绘文件出错", exception.Message + "\n" + exception.StackTrace, () => { }, "确认");
                return;
            }
            GlobalSetting.backgroundImage = sprite;

            UniTask a = UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                try
                {
                    Main.music = await Util.ReadMusicAsAudioClip(GlobalSetting.musicPath);
                }
                catch (ArgumentException)
                {
                    InGameUIManager.ShowModalWindowWithClose("错误", "检测到音频文件为不支持的flac格式",
                        () => { PopupMessageManager.Instance.ChangeContent(""); }, "确认");
                }
                catch (Exception e)
                {
                    InGameUIManager.ShowModalWindowWithClose("读取音频文件出错", e.Message + "\n" + e.StackTrace, () => { },
                        "确认");
                }
            });
            UniTask.WhenAll(a);

            GlobalSetting.usingApi = false;

            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Yande)
            {
                GlobalSetting.chartName = "♡枇杷树上挂♡粒粒油滴下♡让我们一起守护最好的枇杷油♡";
                GlobalSetting.difficulty = "枇杷油嘿嘿枇杷油";
            }
            else if (GlobalSetting.YayaKawaii == GlobalSetting.YayaMode.绝冲)
            {
                GlobalSetting.chartName = "夜夜爱的嗫中毒";
                GlobalSetting.difficulty = "夜夜ღ醉可爱";
            }

            PopupMessageManager.Instance.Clear();
            SceneTransit.Instance.JumpScene("LoadInto");
        }

        public async void EnterGame()
        {
            if (loading || infoDropdown.options.Count == 0)
                return;
            loading = true;
            string phiraInfoPath = Path.Combine(tempPath, "info.yml");
            phiraInfoData = null;
            if (File.Exists(phiraInfoPath))
            {
                IDeserializer deserializer = new DeserializerBuilder().Build();
                phiraInfoData = deserializer.Deserialize<PhiraInfoData>(await File.ReadAllTextAsync(phiraInfoPath));
                GameObject.Find("DiffInput").GetComponent<InputField>().text = phiraInfoData.level;
            }
            
            GlobalSetting.chartName = chartNameUI.GetComponent<InputField>().text.Trim();
//#if UNITY_EDITOR || UNITY_STANDALONE_WIN
//            GlobalSetting.chartpath = tempPath + "\\" + chartPathDropdown.captionText.text;
//            GlobalSetting.musicPath = tempPath + "\\" + musicPathDropdown.captionText.text;
//            GlobalSetting.illustrationPath = tempPath + "\\" + illustrationPathDropdown.captionText.text;
//#else
            //tempPath = Path.Combine(internalPath, tempPath.Substring(tempPath.IndexOf("/0") + 2, tempPath.Length));
            // chart settings
            GlobalSetting.chartPath = Path.Combine(tempPath, chartPathDropdown.captionText.text).Replace('\\', '/');
            GlobalSetting.musicPath = Path.Combine(tempPath, musicPathDropdown.captionText.text).Replace('\\', '/');
            GlobalSetting.illustrationPath =
                Path.Combine(tempPath, illustrationPathDropdown.captionText.text).Replace('\\', '/');
//#endif
            PlayerPrefs.SetString("chartFolderPath", tempPath);
            PlayerPrefs.Save();
            GlobalSetting.difficulty = GameObject.Find("DiffInput").GetComponent<InputField>().text;
            GlobalSetting.ReadUserSettings();

            HitSoundManager.Init();

            if (phiraInfoData == null)
            {
                var infoPath = Path.Combine(tempPath, "info.txt");
                if (File.Exists(infoPath))
                {
                    GlobalSetting.infoTxt = new InfoTxtReader(infoPath);
                    GlobalSetting.chartName = GlobalSetting.infoTxt.GetName();
                    GlobalSetting.difficulty = GlobalSetting.infoTxt.GetDifficulty();
                }
                else
                {
                    GlobalSetting.infoTxt = null;
                }
            }
            else GlobalSetting.infoTxt = null;


            var extraJsonPath = Path.Combine(tempPath, "extra.json");
            if (File.Exists(extraJsonPath))
            {
                InGameUIManager.ShowModalWindowWithClose("检测到extra.json",
                    "检测到extra.json, 暂时只支持内置shader的使用，不支持global属性，确认使用extra.json吗？",
                    () =>
                    {
                        GlobalSetting.extraJson = File.ReadAllText(extraJsonPath);
                        LoadChart();
                    },
                    "使用",
                    () =>
                    {
                        GlobalSetting.extraJson = "";
                        LoadChart();
                    },
                    "不使用");
                return;
            }

            GlobalSetting.extraJson = "";

            //We load chart from here.
            LoadChart();
        }

        private async void LoadChart()
        {
            GlobalSetting.chartFolderPath = PlayerPrefs.GetString("chartFolderPath", "");
            await Main.InitChartAuto(GlobalSetting.chartPath).ConfigureAwait(false);
            Main.OverloadInfoWithPhiraYaml(phiraInfoData);
            if (GlobalSetting.PepoyoDaisuki == GlobalSetting.PepoyoMode.Poyoroid_utsu &&
                (GlobalSetting.composer.ToLowerInvariant().Contains("pepoyo") ||
                 GlobalSetting.composer.Contains("ぺぽよ") || GlobalSetting.composer.Contains("ペポヨ")))
            {
                GlobalSetting.PepoyoDaisuki = GlobalSetting.PepoyoMode.Yande;
            }

            loaded = true;
            await UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                PopupMessageManager.Instance.ChangeContent("loading...");
            });
            for (int i = 0; i < 41; i++)
            {
                await new WaitForEndOfFrame();
            }
        }

        public void OnClickPath()
        {
            chartPathDropdown.ClearOptions();
            musicPathDropdown.ClearOptions();
            illustrationPathDropdown.ClearOptions();
            chartPathDropdown.AddOptions(GetFileName(tempPath, ".json", ".pec"));
            musicPathDropdown.AddOptions(GetFileName(tempPath, ".wav", ".ogg", ".mp3"));
            illustrationPathDropdown.AddOptions(GetFileName(tempPath, ".png", ".bmp", ".jpg", ".jpeg"));
            try
            {
                string t = GetFileName(tempPath, "line.csv").FirstOrDefault()?.text;
                GlobalSetting.lineImage = new CSVReader(Path.Combine(tempPath, t));
            }
            catch
            {
                GlobalSetting.lineImage = null;
            }
        }

        public static List<Dropdown.OptionData> GetFileName(string path, params string[] typeE)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            List<string> types = typeE.Select(type => type.ToLower().Trim()).ToList();

            return (from f in root.GetFiles()
                where types.Contains(Path.GetExtension(f.FullName).ToLower().Trim())
                select new Dropdown.OptionData(f.Name.Trim())).ToList();
        }

        public static List<Dropdown.OptionData> GetFolders(string path)
        {
            List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
            DirectoryInfo root = new DirectoryInfo(path);
            foreach (DirectoryInfo f in root.GetDirectories())
            {
#if UNITY_IPHONE && !UNITY_EDITOR
                if (!f.Name.Trim().StartsWith('.'))
                {
                    list.Add(new Dropdown.OptionData(f.Name.Trim()));
                }
#else
                list.Add(new Dropdown.OptionData(f.Name.Trim()));
#endif
            }

            return list;
        }

        public void SpeedChange()
        {
            speed = float.Parse(GameObject.Find("SpeedDropdown").GetComponent<Dropdown>().captionText.text.Trim('x'));
            GlobalSetting.noteSpeedFactor = speed;
        }

        public void OnChangeDropdown()
        {
            if (infoDropdown.options.Count == 0) return;
            string t = infoDropdown.captionText.text;

            tempPath = Path.Combine(PlayerPrefs.GetString("file_path") /*Application.persistentDataPath*/, t);

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
            FileBrowser.SetFilters(false, ".pez");
            FileBrowser.ShowLoadDialog(OnLoadPezSucceeded, () => { }, FileBrowser.PickMode.Files, false,
                PlayerPrefs.GetString("file_path", Application.persistentDataPath), "", "选择Pez...", "确定");
        }

        private void OnLoadPezSucceeded(string[] paths)
        {
            string zipFile = paths[0];
            string songFolderName = Path.GetFileNameWithoutExtension(zipFile);
            string destFolderPath = Path.Combine(PlayerPrefs.GetString("file_path"), songFolderName);
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