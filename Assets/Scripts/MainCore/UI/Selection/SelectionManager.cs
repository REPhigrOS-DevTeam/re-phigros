using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MainCore.Common;
using MainCore.Serialized;
using MainCore.UI.Utils;
using MainCore.Utilities;
using Newtonsoft.Json;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI.Selection
{
    public class SelectionManager : MonoSingleton<SelectionManager>
    {
        [SerializeField] private SelectionInfoBinder songCardPrototype;
        [SerializeField] private RectTransform contentTransform;
        [SerializeField] private Button back, start, setting, import;
        [SerializeField] private RawImage backgroundImage;
        [SerializeField] private RawImage fallbackBackgroundImage;
        [SerializeField] private PullableScrollRect refreshScroll;
        
        private BeatmapCatalog _catalog;
        private bool _loading;

        public BeatmapCatalog Catalog => _catalog;
        
        private void Start()
        {
            GlobalSetting.Reset();
            back.onClick.AddListener(() =>
            {
                backgroundImage.texture = fallbackBackgroundImage.texture;
                SelectionPreview.Reset();
                SceneTransit.Instance.Back(useOldTransition: false);
            });
            start.onClick.AddListener(StartPlay);
            setting.onClick.AddListener(() =>
            {
                SceneTransit.Instance.LoadAdditiveScene("SettingsScene");
            });
            import.onClick.AddListener(TryUnzipPez);
            refreshScroll.PullDistanceRequiredRefresh = 150f;
            refreshScroll.OnRefresh.AddListener(RefreshGameFolder);
            ReadCatalog();
            RefreshGameFolder();
        }

        private async void StartPlay()
        {
            if (SelectionPreview.SelectedInfo == null || _loading)
            {
                return;
            }
            _loading = true;
            GlobalSetting.SetBeatmap(SelectionPreview.SelectedInfo);
            var success = await SelectionPreview.SelectedInfo.LoadBeatmap();
            if (!success)
            {
                _loading = false;
                PopupMessageManager.Instance.ChangeContent("Failed, maybe essential files missing.");
                return;
            }
            PopupMessageManager.Instance.ChangeContent("Done.");
            await UniTask.Delay(1000);
            PopupMessageManager.Instance.Clear();
            SelectionPreview.Reset();
            SceneTransit.Instance.LoadScene("LoadingScene", 0);
        }

        public async void RefreshGameFolder()
        {
            //Clear previous cards
            //0-2 is Refresh Indicator! DO NOT CLEAR IT
            //3 is prototype! DO NOT CLEAR IT
            for (var i = 4; i < contentTransform.childCount; i++)
            {
                Destroy(contentTransform.GetChild(i).gameObject);
            }
            
            var folders = GetFolders(Util.DataPath);
            var newDict = new Dictionary<string, BeatmapInfo>();
            var failedFolders = new List<string>();
            
            foreach (var folder in folders)
            {
                var fullPath = Path.Combine(Util.DataPath, folder);
                if (!_catalog.Infos.TryGetValue(folder, out var info))
                {
                    info = await new BeatmapInfo().ReloadFromPathFallback(fullPath);

                    if (info == null)
                    {
                        failedFolders.Add(folder);
                        //InGameUIManager.ShowModalWindowWithClose("错误", $"未能成功读取谱面 {folder}\n请自行处理，仅支持info.txt, info.yml, RPE json", () => { }, "确定");
                        continue;
                    }
                }
                info.BasePath = fullPath;
                info.Illustration = null;
                newDict.Add(folder, info);
            }

            if (failedFolders.Count != 0)
            {
                if (failedFolders.Count > 5)
                {
                    failedFolders = failedFolders.Take(5).ToList()
                        .Append($"...以及其余{failedFolders.Count - 5}项").ToList();
                }
                InGameUIManager.ShowModalWindowWithClose("以下谱面未成功读取，请自行检查", string.Join('\n', failedFolders), () => { }, "确定");
            }

            _catalog.Infos = newDict;

            foreach (var p in _catalog.Infos)
            {
                var binder = Instantiate(songCardPrototype, contentTransform);
                binder.SetInfo(p.Value);
                binder.gameObject.SetActive(true);
            }
            
            SelectionScrollPool.Instance.Warmup();
            
            SaveCatalog();
        }

        private void ReadCatalog()
        {
            var catalogPath = Path.Combine(Application.persistentDataPath, "beatmap_catalog.json");
            _catalog = !File.Exists(catalogPath)
                ? new BeatmapCatalog() 
                : JsonConvert.DeserializeObject<BeatmapCatalog>(File.ReadAllText(catalogPath));
        }

        private void SaveCatalog()
        {
            var catalogPath = Path.Combine(Application.persistentDataPath, "beatmap_catalog.json");
            File.WriteAllText(catalogPath, JsonConvert.SerializeObject(_catalog));
        }
        
        private void TryUnzipPez()
        {
            OpenFile.LoadFile(zipFile =>
                {
                    GameUtils.UnzipChartArchive(zipFile, RefreshGameFolder, InGameUIManager.ShowModalWindowWithCloseFromWindowInfo);
                }, () => { }, new[] { new ExtensionFilter("RPE谱包", "pez") }, null,
                "选择Pez...", "确定");
        }
        
        private static List<string> GetFolders(string path)
        {
            var list = new List<string>();
            var root = new DirectoryInfo(path);
            if (!root.Exists) return list;
            foreach (var f in root.GetDirectories())
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
    }
}