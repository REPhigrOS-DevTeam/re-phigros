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

namespace MainCore.UI
{
    public class SelectUIControl : MonoBehaviour
    {
        public GameObject chartNameUI;
        public Dropdown chartPathDropdown;
        public Dropdown musicPathDropdown;
        public Dropdown illustrationPathDropdown;
        public Button titleButton;

        private int clickCounter;
        private bool loaded;
        private bool loading;
        private PhiraInfoData phiraInfoData;
        private UnityWebRequest request;
        private UnityWebRequest request2;
        private bool requested;
        private float speed;
        private string tempPath;

        //private string internalPath = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("/Android"));

        // Start is called before the first frame update
        void Start()
        {
            if (PlayerPrefs.GetInt("half_res", 0) == 1)
            {
                Debug.Log("Half Resolution Mode Enabled");
                var currentRes = GlobalSetting.OriginResolution;
                currentRes.height /= 2;
                currentRes.width /= 2;
                Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreenMode);
            }
            else
            {
                Debug.Log("Half Resolution Mode Disabled");
                var currentRes = GlobalSetting.OriginResolution;
                Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreenMode);
            }


#if UNITY_IPHONE && !UNITY_EDITOR
            PlayerPrefs.SetString("file_path", Application.persistentDataPath);
            PlayerPrefs.Save();
#endif

            //PlayerPrefs.SetString("chartFolderPath", tempPath);
            GameObject.Find("DiffInput").GetComponent<InputField>().text =
                PlayerPrefs.GetString("difficultyName", "SP Lv.?");
            chartNameUI.GetComponent<InputField>().text = PlayerPrefs.GetString("chartName", "Untitled");
            RefreshGameFolder();

            Application.targetFrameRate = PlayerPrefs.GetInt("refresh_rate", 60);

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
                    SceneTransit.Instance.TransitTo("WahtThe");
                });
            }

            GlobalSetting.Reset();
        }

        private void RefreshGameFolder()
        {
            GameObject.Find("InfoDropdown").GetComponent<Dropdown>().options =
                GetFolders(PlayerPrefs.GetString("file_path"));
            OnChangeDropdown();
        }

        private void Update()
        {
            if (!loaded) return;
            if (!requested)
            {
                try
                {
                    FileStream fileStream = new FileStream(GlobalSetting.musicPath, FileMode.Open, FileAccess.Read,
                        FileShare.Read);
                    byte[] fileHead = new byte[4];
                    if (fileStream.Read(fileHead, 0, fileHead.Length) != fileHead.Length)
                        throw new Exception("Illegal Music File");
                    fileStream.Close();
                    if (fileHead[0] == 0x66 && fileHead[1] == 0x4C && fileHead[2] == 0x61 && fileHead[3] == 0x43)
                    {
                        InGameUIManager.ShowModalWindowWithClose("错误", "检测到音频文件为不支持的flac格式",
                            () => { PopupMessageManager.Instance.ChangeContent(""); }, "确认");
                        loaded = false;
                        loading = false;
                        return;
                    }

                    Uri.TryCreate(GlobalSetting.illustrationPath, UriKind.Absolute, out var uri);
                    request = UnityWebRequestTexture.GetTexture(uri);
                    request.SendWebRequest();
                    var suffix = Path.GetExtension(GlobalSetting.musicPath).ToLower();
                    Uri.TryCreate(GlobalSetting.musicPath, UriKind.Absolute, out uri);
                    request2 = UnityWebRequestMultimedia.GetAudioClip(uri, suffix switch
                    {
                        ".wav" => AudioType.WAV,
                        ".ogg" => AudioType.OGGVORBIS,
                        ".mp3" => AudioType.MPEG,
                        _ => AudioType.UNKNOWN
                    });
                    request2.SendWebRequest();
                    requested = true;
                }
                catch (Exception e)
                {
                    PopupMessageManager.Instance.ChangeContent($"{e.Message}");
                }
            }

            if (!loading) return;
            if (!request.isDone)
            {
                PopupMessageManager.Instance.ChangeContent(
                    $"Loading Illustration: {request.downloadProgress * 100:F2}%");
                return;
            }

            if (!request2.isDone)
            {
                PopupMessageManager.Instance.ChangeContent($"Loading Music: {request2.downloadProgress * 100:F2}%");
                return;
            }

            loading = false;
            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    InGameUIManager.ShowModalWindowWithClose("读取曲绘文件出错", request.downloadHandler.error, () => { },
                        "确认");
                }
                else
                {
                    InGameUIManager.ShowModalWindowWithClose("读取曲绘文件出错", request.error, () => { }, "确认");
                }

                return;
            }

            var texture = (request.downloadHandler as DownloadHandlerTexture).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            GlobalSetting.backgroundImage = sprite;

            if (request2.result != UnityWebRequest.Result.Success)
            {
                InGameUIManager.ShowModalWindowWithClose("读取音频文件出错", request2.error, () => { }, "确认");
                return;
            }

            bool error = false;
            string errorMessage = "";
            Application.logMessageReceived += (condition, stacktrace, type) =>
            {
                if (type == LogType.Error)
                {
                    error = true;
                    errorMessage = condition + "\n" + "<size=20>StackTrace: \n" + stacktrace + "</size>";
                }
            };

            Main.music = (request2.downloadHandler as DownloadHandlerAudioClip).audioClip;

            if (error)
            {
                InGameUIManager.ShowModalWindowWithClose("读取音频文件出错", errorMessage, () => { }, "确认");
                return;
            }

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
            SceneTransit.Instance.TransitTo("LoadInto");
        }

        public async void EnterGame()
        {
            if (loading)
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
            GlobalSetting.chartPath = Path.Combine(tempPath, chartPathDropdown.captionText.text).Replace('\\', '/');
            GlobalSetting.musicPath = Path.Combine(tempPath, musicPathDropdown.captionText.text).Replace('\\', '/');
            GlobalSetting.illustrationPath =
                Path.Combine(tempPath, illustrationPathDropdown.captionText.text).Replace('\\', '/');
//#endif
            PlayerPrefs.SetString("chartFolderPath", tempPath);
            PlayerPrefs.Save();
            GlobalSetting.highLight = PlayerPrefs.GetInt("high_light", 0) == 1; //highlightToggle.isOn;
            GlobalSetting.difficulty = GameObject.Find("DiffInput").GetComponent<InputField>().text;
            GlobalSetting.userOffset =
                PlayerPrefs.GetFloat("chart_offset", 0) /
                1000f; //int.Parse(GameObject.Find("DelayInput").GetComponent<InputField>().text) / 1000f;
            GlobalSetting.autoPlay =
                PlayerPrefs.GetInt("auto_play", 0) == 1; //GameObject.Find("AutoToggle").GetComponent<Toggle>().isOn;
            GlobalSetting.isMirror =
                PlayerPrefs.GetInt("mirror", 0) == 1; //GameObject.Find("MirrorToggle").GetComponent<Toggle>().isOn;
            GlobalSetting.disableBlur = PlayerPrefs.GetInt("blur", 0) == 1;
            GlobalSetting.is3D =
                false; //PlayerPrefs.GetInt("3d", 0) == 1;//GameObject.Find("3DToggle").GetComponent<Toggle>().isOn;
            GlobalSetting.postProcessing =
                PlayerPrefs.GetInt("post_processing", 0) ==
                1; //GameObject.Find("PostProcessingToggle").GetComponent<Toggle>().isOn;
            GlobalSetting.globalNoteScale = PlayerPrefs.GetFloat("note_size", 0.25f) * GameUtils.ScreenDelta;
            GlobalSetting.recordMode = PlayerPrefs.GetInt("record_mode", 0) == 1;
            GlobalSetting.hitVolume = PlayerPrefs.GetFloat("hit_volume", 1f);
            GlobalSetting.maskAlpha = PlayerPrefs.GetFloat("mask_alpha", .5f);
            GlobalSetting.fxaaEnabled = PlayerPrefs.GetInt("fxaa", 0) == 1;

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


            var extraJsonPath = Path.Combine(tempPath, "extra.json");
            if (File.Exists(extraJsonPath) && GlobalSetting.useShader)
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
        }

        public void OnClickPath()
        {
            chartPathDropdown.options = GetFileName(tempPath, ".json", ".pec");
            musicPathDropdown.options = GetFileName(tempPath, ".wav", ".ogg", ".mp3");
            illustrationPathDropdown.options = GetFileName(tempPath, ".png", ".bmp", ".jpg", ".jpeg");
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
            List<string> types = typeE.ToList();

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
            string t = GameObject.Find("InfoDropdown").GetComponent<Dropdown>().captionText.text;

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

        public void EnterSettings()
        {
            SceneTransit.Instance.TransitTo("SettingsScene");
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
                UnZip(zipFile, destFolderPath);
                string externalTextureZip = destFolderPath + "/" + "texture.zip";
                if (File.Exists(externalTextureZip))
                {
                    UnZip(externalTextureZip, destFolderPath);
                    File.Delete(externalTextureZip);
                }

                InGameUIManager.ShowModalWindowWithClose("提示", "解压成功", () => { }, "确定");
                RefreshGameFolder();
            }
        }

        /// <summary>
        /// ZIP:解压一个zip文件
        /// add yuangang by 2016-06-13
        /// </summary>
        /// <param name="ZipFile">需要解压的Zip文件（绝对路径）</param>
        /// <param name="TargetDirectory">解压到的目录</param>
        /// <param name="OverWrite">是否覆盖已存在的文件</param>
        public static void UnZip(string ZipFile, string TargetDirectory, bool OverWrite = true)
        {
            TargetDirectory = TargetDirectory.Replace("\\", "/");
            //如果解压到的目录不存在，则报错
            if (!Directory.Exists(TargetDirectory))
            {
                Directory.CreateDirectory(TargetDirectory);
            }

            //目录结尾
            if (!TargetDirectory.EndsWith("/"))
            {
                TargetDirectory = String.Concat(TargetDirectory, "/");
            }

            using ZipInputStream zipfiles = new ZipInputStream(File.OpenRead(ZipFile));
            ZipEntry theEntry;

            while ((theEntry = zipfiles.GetNextEntry()) != null)
            {
                string directoryName = "";
                string pathToZip = "";
                pathToZip = theEntry.Name;

                if (pathToZip != "")
                    directoryName = Path.GetDirectoryName(pathToZip) + "/";

                string fileName = Path.GetFileName(pathToZip);

                Directory.CreateDirectory(TargetDirectory + directoryName);

                if (fileName == "") continue;
                if ((!File.Exists(TargetDirectory + directoryName + fileName) || !OverWrite) &&
                    (File.Exists(TargetDirectory + directoryName + fileName))) continue;
                using FileStream streamWriter = File.Create(TargetDirectory + directoryName + fileName);
                int size = 2048;
                byte[] data = new byte[2048];
                while (true)
                {
                    size = zipfiles.Read(data, 0, data.Length);

                    if (size > 0)
                        streamWriter.Write(data, 0, size);
                    else
                        break;
                }

                streamWriter.Close();
            }

            zipfiles.Close();
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