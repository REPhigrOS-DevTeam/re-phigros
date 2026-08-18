using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Lean.Gui;
using MainCore.Common;
using MainCore.Data;
using MainCore.ECS;
using MainCore.UI;
using MainCore.UI.Utils;
using MainCore.Utilities;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Settings
{
    public class SettingManager : MonoBehaviour
    {
        [SerializeField] private Button saveNExit;
        [SerializeField] private LeanButton delayCorrectionEnter;
        [SerializeField] private LeanButton dspEnter;
        [SerializeField] private LeanButton logOut;
        [SerializeField] private InputField_File_Selector dataPath;
        [SerializeField] private Transform broadCastTarget;
        [SerializeField] private LeanToggle[] toggles;
        [SerializeField] private RectTransform internalSkinParent, externalSkinParent;
        [SerializeField] private GameObject skinItemPrefab;
        [SerializeField] private LeanButton openSkinSelector, closeSkinSelector;
        [SerializeField] private GameObject skinSelectorCanvas, skinPreview;
        [SerializeField] private SkinPreview skinPreviewer;
        [SerializeField] private Button displaySkinInfo, deleteSkin;
        [SerializeField] private Transform hitEffectPos;
        [SerializeField] private LeanButton openAbout, closeAbout;
        [SerializeField] private GameObject aboutCanvas;
        [SerializeField] private Text aboutText;
        [SerializeField] private Slider_Float_Setting delaySlider;
        private readonly Dictionary<Skin, SkinItem> _internalSkinItems = new();
        private readonly Dictionary<string, SkinItem> _externalSkinItems = new();
        private bool _selectedIsExternal;
        private string _selectedId = "-1";

        private const string SceneName = "SettingsScene";

        private void Start()
        {
            // 按钮注册
            displaySkinInfo.onClick.AddListener(() =>
            {
                InGameUIManager.ShowModalWindowWithClose("信息",
                    $"名称：{GlobalSetting.CurrentSkinInfo.skinName}\n" +
                    $"作者：{GlobalSetting.CurrentSkinInfo.author}\n" +
                    $"介绍：{GlobalSetting.CurrentSkinInfo.description}", () => { }, "确定");
            });
            deleteSkin.onClick.AddListener(() =>
            {
                if (GlobalSetting.CurrentSkinInfo.isExternal)
                {
                    InGameUIManager.ShowModalWindowWithClose("提示", "确定要删除吗？", () =>
                    {
                        string id = GlobalSetting.CurrentSkinInfo.id;
                        UpdateSelectedSkinItem(false, "0");
                        SkinManager.Instance.DeleteSkinInfo(id);
                        RefreshExternalSkins();
                    }, "确定", () => { }, "取消");
                }
                else
                {
                    InGameUIManager.ShowModalWindowWithClose("错误", "不可删除内置皮肤", () => { }, "好");
                }
            });
            openSkinSelector.OnClick.AddListener(() =>
            {
                skinSelectorCanvas.SetActive(true);
                skinPreview.SetActive(true);
            });
            closeSkinSelector.OnClick.AddListener(() =>
            {
                skinSelectorCanvas.SetActive(false);
                skinPreview.SetActive(false);
            });
            openAbout.OnClick.AddListener(() =>
            {
                aboutCanvas.SetActive(true);
            });
            closeAbout.OnClick.AddListener(() =>
            {
                aboutCanvas.SetActive(false);
            });
            saveNExit.onClick.AddListener(SaveNExit);
            dspEnter.OnClick.AddListener(IntoDSP);
            delayCorrectionEnter.OnClick.AddListener(IntoDelayCorrection);
            if (GlobalSetting.IsOffline)
            {
                logOut.transform.Find("Cap").Find("Text").gameObject.GetComponent<Text>().text = "登录";
                logOut.OnClick.AddListener(LogIn);
            }
            else
            {
                logOut.transform.Find("Cap").Find("Text").gameObject.GetComponent<Text>().text = "登出";
                logOut.OnClick.AddListener(LogOut);
            }

            if (!PlayerPrefs.HasKey(dataPath.BaseData.DataTag))
            {
                dataPath.BaseData.SetValue($"{Application.persistentDataPath}");
            }
#if UNITY_IPHONE && !UNITY_EDITOR
            dataPath.Lock();
#endif
            // 彩蛋们
            /*SpecialEvent caiDan1 = new SpecialEvent(toggles,
                new[]
                {
                    (int)YayaModeInSettings.吹鸣, (int)YayaModeInSettings.森闲, (int)YayaModeInSettings.光焰,
                    (int)YayaModeInSettings.天崄
                }, () =>
                {
                    Debug.Log("结！");
                    InGameUIManager.ShowModalWindowWithClose("<size=15>再去主界面标题标题点十下</size>", "这都能被你发现", () => { },
                        "芜湖~");
                    GlobalSetting.YayaKawaii = GlobalSetting.YayaMode.结;
                });
            SpecialEvent caiDan2 = new SpecialEvent(toggles,
                new[]
                {
                    (int)PepoyoModeInSettings.Jikkentai, (int)PepoyoModeInSettings.Neurose,
                    (int)PepoyoModeInSettings.Waraninja, (int)PepoyoModeInSettings.Anrakushi
                }, () =>
                {
                    Debug.Log("躁！");
                    InGameUIManager.ShowModalWindowWithClose("<size=15>再去主界面标题标题点十下</size>", "这也被你发现啦", () => { },
                        "枇杷树上挂 粒粒油滴下");
                    GlobalSetting.PepoyoDaisuki = GlobalSetting.PepoyoMode.Poyoroid_sou;
                });*/
            // SpecialEvent qiYongExtraJson = new SpecialEvent(toggles, new[] { 5, 6, 4, 8 }, () => { });
//             OnSkinChanged(skinDropdown.value);
// #if !RELEASE_VERSION || UNITY_EDITOR
//             skinDropdown.AddOptions(new List<string> { "Phira", "萨卡斑甲鱼" });
// #endif
            // 皮肤
            skinSelectorCanvas.SetActive(false);
            skinPreview.SetActive(false);
            for (int i = 0; i < internalSkinParent.childCount; i++)
            {
                Destroy(internalSkinParent.GetChild(i).gameObject);
            }
#if !RELEASE_VERSION || UNITY_EDITOR
            int internalMax = 5;
#else
            int internalMax = 3;
#endif
            if (!GlobalSetting.IsOffline && GlobalSetting.Username.ToLowerInvariant() is "sky" or "greenball233" or "debug") internalMax = Math.Max(4, internalMax);
            for (var i = 0; i < internalMax; i++)
            {
                var o = Instantiate(skinItemPrefab, internalSkinParent);
                o.name = ((Skin)i).ToString();
                var skinItem = o.GetComponent<SkinItem>();
                skinItem.Init(this, false, i.ToString(), ((Skin)i).ToString());
                _internalSkinItems.Add((Skin)i, skinItem);
            }

            RefreshExternalSkins();

            if (GlobalSetting.CurrentSkinInfo.isExternal)
            {
                _externalSkinItems[GlobalSetting.CurrentSkinInfo.id].GetComponent<Button>().onClick.Invoke();
            }
            else
            {
                _internalSkinItems[GlobalSetting.CurrentSkinInfo.skin]?.GetComponent<Button>().onClick.Invoke();
            }
            
            // 其他
            aboutCanvas.SetActive(false);
            var aboutTextAsset = Resources.Load<TextAsset>("Others/About");
            aboutText.text = aboutTextAsset.text;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)aboutText.rectTransform.parent);
            Resources.UnloadAsset(aboutTextAsset);
        }

        private void LogOut()
        {
            InGameUIManager.ShowModalWindowWithClose("提示", "确定要登出吗？", () =>
            {
                GlobalSetting.Username = "";
                GlobalSetting.VerifyToken = "";
                logOut.transform.Find("Cap").Find("Text").gameObject.GetComponent<Text>().text = "登录";
                logOut.OnClick.RemoveListener(LogOut);
                logOut.OnClick.AddListener(LogIn);
            }, "确定", () => { }, "取消");
        }

        private void LogIn()
        {
            SceneTransit.Instance.LeaveAdditiveScene(SceneName);
            SceneTransit.Instance.LoadScene("LoginScene");
        }

        private void RefreshExternalSkins()
        {
            for (var i = 0; i < externalSkinParent.childCount; i++)
            {
                Destroy(externalSkinParent.GetChild(i).gameObject);
            }

            _externalSkinItems.Clear();
            var skinSummaries = SkinManager.Instance.GetSkinSummaries();
            foreach (var skinSummary in skinSummaries)
            {
                var go = Instantiate(skinItemPrefab, externalSkinParent);
                go.name = skinSummary.ID;
                var skinItem = go.GetComponent<SkinItem>();
                skinItem.Init(this, true, skinSummary.ID, skinSummary.Name);
                _externalSkinItems.Add(skinSummary.ID, skinItem);
            }

            var addGameObject = Instantiate(skinItemPrefab, externalSkinParent);
            addGameObject.name = "Add";
            var addSkinItem = addGameObject.GetComponent<SkinItem>();
            addSkinItem.Init(this, true, "", "＋");

            if (_selectedIsExternal)
            {
                _externalSkinItems[_selectedId]?.SetSelected(true, _selectedId);
            }
        }

        private void IntoDSP()
        {
            string qwq = null;
            if (PlayerPrefs.HasKey(dataPath.BaseData.DataTag)) qwq = PlayerPrefs.GetString(dataPath.BaseData.DataTag);
            broadCastTarget.BroadcastMessage("SaveValue");
            PlayerPrefs.Save();
            if (qwq != null) PlayerPrefs.SetString(dataPath.BaseData.DataTag, qwq);
            SceneTransit.Instance.LoadAdditiveScene("DSPScene");
        }
        
        private void IntoDelayCorrection()
        {
            string qwq = null;
            if (PlayerPrefs.HasKey(dataPath.BaseData.DataTag)) qwq = PlayerPrefs.GetString(dataPath.BaseData.DataTag);
            broadCastTarget.BroadcastMessage("SaveValue");
            PlayerPrefs.Save();
            if (qwq != null) PlayerPrefs.SetString(dataPath.BaseData.DataTag, qwq);
            DelayCorrection.DelaySlider = delaySlider;
            SceneTransit.Instance.LoadAdditiveScene("DelayCorrectionScene");
        }

        private void SaveNExit()
        {
            broadCastTarget.BroadcastMessage("SaveValue");
            PlayerPrefs.Save();
            
#if UNITY_IPHONE && !UNITY_EDITOR
            PlayerPrefs.DeleteKey("file_path");
            PlayerPrefs.Save();

            if (!Directory.Exists(Util.DataPath))
            {
                InGameUIManager.ShowModalWindowWithClose("故意的是吧", "你这文件夹都不存在啊", () => { }, "确认");
                return;
            }
#endif

            
            var currentRes = GlobalSetting.OriginResolution;
            if (PlayerPrefsExtension.GetBoolean("half_res", false))
            {
                Debug.Log("[SettingManager] Half Resolution Mode Enabled");
                Screen.SetResolution(currentRes.width /= 2, currentRes.height /= 2, Screen.fullScreenMode);
            }
            else
            {
                Debug.Log("[SettingManager] Half Resolution Mode Disabled");
                Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreenMode);
            }

            Application.targetFrameRate = PlayerPrefs.GetInt("refresh_rate", 60);
            
            SceneTransit.Instance.LeaveAdditiveScene(SceneName);
        }

        public void UpdateSelectedSkinItem(bool isExternal, string id)
        {
            if (isExternal && id == "")
            {
                OpenFile.LoadFile(AddSkinFromPackage, () => { },
                    new[] { new ExtensionFilter("Phira皮肤包", "zip") }, null, "选择皮肤包…", "确定");
                return;
            }

            if (_selectedIsExternal == isExternal && _selectedId == id) return;
            var newSkinInfo = SkinManager.Instance.GetSkinInfo(isExternal, id);
            if (newSkinInfo == null)
            {
                if (!isExternal) throw new Exception("not should've been here...??");
                SkinManager.Instance.DeleteSkinInfo(id);
                RefreshExternalSkins();
                return;
            }

            _selectedIsExternal = isExternal;
            _selectedId = id;
            foreach (var internalSkinItem in _internalSkinItems.Values)
            {
                internalSkinItem.SetSelected(isExternal, id);
            }

            foreach (var externalSkinItem in _externalSkinItems.Values)
            {
                externalSkinItem.SetSelected(isExternal, id);
            }

            Debug.Log($"[SkinManager] Switching to skin: {newSkinInfo.skinName}");
            
            GlobalSetting.CurrentSkinInfo = newSkinInfo;
            HitSoundManager.Instance.RefreshHitSounds();
            OnSkinChanged();
            
            Debug.Log($"[SkinManager] Done.");
        }

        private void OnSkinChanged()
        {
            SkinManager.Instance.SaveCurrentSkinInfo();
            skinPreviewer.UpdateSkin();
            EffectSystemManager.Instance.UpdateSkin();
        }

        private async void AddSkinFromPackage(string path)
        {
            await UniTask.SwitchToMainThread();
            string tmpDirPath = Application.temporaryCachePath + "/tmpSkinPackage";
            string dirPath = $"{tmpDirPath}/{Path.GetFileNameWithoutExtension(path)}";
            if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);
            try
            {
                ZipUtils.UnZip(await File.ReadAllBytesAsync(path),
                    tmpDirPath + $"/{Path.GetFileNameWithoutExtension(path)}");
            }
            catch (IOException)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "无法读取文件", () => { }, "确定");
                return;
            }

            SkinInfo readSkin = await GameUtils.ReadSkin(dirPath);
            if (readSkin == null)
            {
                Directory.Delete(tmpDirPath + $"/{Path.GetFileNameWithoutExtension(path)}", true);
                return;
            }
            SkinManager.Instance.AddSkinInfo(dirPath, readSkin);
            RefreshExternalSkins();
        }

        public void PlayHitSound(int id)
        {
            HitSoundManager.Instance.Play(id, 0.5f);
        }

        public void PlayHitEffect(int type)
        {
            var tmp = GlobalSetting.GlobalNoteScale;
            GlobalSetting.GlobalNoteScale = SkinPreview.Size;
            var hitFxObj = HitEffectManager.GetInstance()
                .GetObj((HitFxJudgeType)type, GlobalSetting.CurrentSkinInfo, true);
            hitFxObj.transform.position = hitEffectPos.position;
            hitFxObj.transform.rotation = Quaternion.identity;
            hitFxObj.PlayEffect();
            GlobalSetting.GlobalNoteScale = tmp;
        }
    }

    public enum YayaModeInSettings
    {
        吹鸣 = 3,
        森闲 = 1,
        光焰 = 2,
        天崄 = 6
    }

    public enum PepoyoModeInSettings
    {
        Jikkentai = 6,
        Neurose = 5,
        Waraninja = 7,
        Anrakushi = 3
    }
}