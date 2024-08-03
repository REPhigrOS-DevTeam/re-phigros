using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MainCore.Common;
using MainCore.Serialized;
using MainCore.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.UI.Selection
{
    public class SelectionManager : MonoBehaviour
    {
        [SerializeField] private SelectionInfoBinder songCardPrototype;
        [SerializeField] private RectTransform contentTransform;
        [SerializeField] private Button back, start, setting;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image fallbackBackgroundImage;
        
        private BeatmapCatalog _catalog;
        private bool _loading = false;
        
        private void Start()
        {
            GlobalSetting.Reset();
            back.onClick.AddListener(() =>
            {
                backgroundImage.sprite = fallbackBackgroundImage.sprite;
                SceneTransit.Instance.Back(useOldTransition: false);
            });
            start.onClick.AddListener(StartPlay);
            setting.onClick.AddListener(() =>
            {
                backgroundImage.sprite = fallbackBackgroundImage.sprite;
                SceneTransit.Instance.LoadScene("SettingsScene");
            });
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
            await SelectionPreview.SelectedInfo.LoadBeatmap();
            SceneTransit.Instance.LoadScene("LoadingScene", 0);
        }

        public async void RefreshGameFolder()
        {
            //Clear previous cards
            //0 is prototype! DO NOT CLEAR IT
            for (var i = 1; i < contentTransform.childCount; i++)
            {
                Destroy(contentTransform.GetChild(i).gameObject);
            }
            
            var folders = GetFolders(Util.DataPath);
            var newDict = new Dictionary<string, BeatmapInfo>();
            foreach (var folder in folders)
            {
                var fullPath = Path.Combine(Util.DataPath, folder);
                if (!_catalog.Infos.TryGetValue(folder, out var info))
                {
                    info = await new BeatmapInfo().ReloadFromPathFallback(fullPath);

                    if (info == null)
                    {
                        InGameUIManager.ShowModalWindowWithClose("错误", $"未能成功读取谱面 {folder}\n请自行处理，仅支持info.txt, info.yml, RPE json", () => { }, "确定");
                        continue;
                    }
                }
                info.BasePath = fullPath;
                newDict.Add(folder, info);
            }

            _catalog.Infos = newDict;

            foreach (var p in _catalog.Infos)
            {
                var binder = Instantiate(songCardPrototype, contentTransform);
                binder.SetInfo(p.Value).NotifyUpdate();
                binder.gameObject.SetActive(true);
            }

            SaveCatalog();
        }

        private void ReadCatalog()
        {
            var catalogPath = Path.Combine(Application.persistentDataPath, "beatmap_catalog.json");
            if (!File.Exists(catalogPath))
            {
                _catalog = new BeatmapCatalog();
            }
            else
            {
                _catalog = JsonConvert.DeserializeObject<BeatmapCatalog>(File.ReadAllText(catalogPath));
            }
        }

        private void SaveCatalog()
        {
            var catalogPath = Path.Combine(Application.persistentDataPath, "beatmap_catalog.json");
            File.WriteAllText(catalogPath, JsonConvert.SerializeObject(_catalog));
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