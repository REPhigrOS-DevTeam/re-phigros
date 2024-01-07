using System;
using System.Collections.Generic;
using System.IO;
using Lean.Gui;
using MainCore.Common;
using MainCore.ECS_ver;
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
        [SerializeField] private LeanButton saveNExit;
        [SerializeField] private LeanButton dspEnter;
        [SerializeField] private LeanButton logOut;
        [SerializeField] private InputField_File_Selector dataPath;
        [SerializeField] private Transform broadCastTarget;
        [SerializeField] private LeanToggle[] toggles;
        [SerializeField] private DelayCorrect delayCorrect;
        [SerializeField] private RectTransform internalSkinParent, externalSkinParent;
        [SerializeField] private GameObject skinItemPrefab;
        [SerializeField] private Button openSkinSelector, closeSkinSelector;
        [SerializeField] private GameObject skinSelectorCanvas, skinPreview;
        [SerializeField] private SkinPreview skinPreviewer;
        [SerializeField] private Button displaySkinInfo, deleteSkin;
        [SerializeField] private Transform hitEffectPos;
        private Dictionary<Skin, SkinItem> internalSkinItems = new();
        private Dictionary<string, SkinItem> externalSkinItems = new();
        private bool selectedIsExternal = false;
        private string selectedId = "-1";
#if UNITY_EDITOR
        public Sprite[] hitFx;
#endif

        void Start()
        {
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
            openSkinSelector.onClick.AddListener(() =>
            {
                skinSelectorCanvas.SetActive(true);
                skinPreview.SetActive(true);
                delayCorrect.SetRunning(false);
            });
            closeSkinSelector.onClick.AddListener(() =>
            {
                skinSelectorCanvas.SetActive(false);
                skinPreview.SetActive(false);
                delayCorrect.SetRunning(true);
            });
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
            saveNExit.OnClick.AddListener(SaveNExit);
            dspEnter.OnClick.AddListener(IntoDSP);
            SpecialEvent caiDan1 = new SpecialEvent(toggles,
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
                });
            // SpecialEvent qiYongExtraJson = new SpecialEvent(toggles, new[] { 5, 6, 4, 8 }, () => { });
//             OnSkinChanged(skinDropdown.value);
// #if !RELEASE_VERSION || UNITY_EDITOR
//             skinDropdown.AddOptions(new List<string> { "Phira", "萨卡斑甲鱼" });
// #endif
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
            if (GlobalSetting.Username.ToLowerInvariant() is "sky" or "greenball233" or "debug") internalMax = Math.Max(4, internalMax);
            for (int i = 0; i < internalMax; i++)
            {
                GameObject o = Instantiate(skinItemPrefab, internalSkinParent);
                o.name = ((Skin)i).ToString();
                SkinItem skinItem = o.GetComponent<SkinItem>();
                skinItem.Init(this, false, i.ToString(), ((Skin)i).ToString());
                internalSkinItems.Add((Skin)i, skinItem);
            }

            RefreshExternalSkins();

            if (GlobalSetting.CurrentSkinInfo.isExternal)
            {
                externalSkinItems[GlobalSetting.CurrentSkinInfo.id].GetComponent<Button>().onClick.Invoke();
            }
            else
            {
                internalSkinItems[GlobalSetting.CurrentSkinInfo.skin]?.GetComponent<Button>().onClick.Invoke();
            }
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
            SceneTransit.Instance.JumpScene("LoginScene");
        }

        private void RefreshExternalSkins()
        {
            for (int i = 0; i < externalSkinParent.childCount; i++)
            {
                Destroy(externalSkinParent.GetChild(i).gameObject);
            }

            externalSkinItems.Clear();
            SkinSummary[] skinSummaries = SkinManager.Instance.GetSkinSummaries();
            foreach (SkinSummary skinSummary in skinSummaries)
            {
                GameObject o = Instantiate(skinItemPrefab, externalSkinParent);
                o.name = skinSummary.id;
                SkinItem skinItem = o.GetComponent<SkinItem>();
                skinItem.Init(this, true, skinSummary.id, skinSummary.name);
                externalSkinItems.Add(skinSummary.id, skinItem);
            }

            GameObject add = Instantiate(skinItemPrefab, externalSkinParent);
            add.name = "Add";
            SkinItem skinItem1 = add.GetComponent<SkinItem>();
            skinItem1.Init(this, true, "", "＋");

            if (selectedIsExternal)
            {
                externalSkinItems[selectedId]?.SetSelected(true, selectedId);
            }
        }

        private void IntoDSP()
        {
            string? qwq = null;
            if (PlayerPrefs.HasKey(dataPath.BaseData.DataTag)) qwq = PlayerPrefs.GetString(dataPath.BaseData.DataTag);
            broadCastTarget.BroadcastMessage("SaveValue");
            PlayerPrefs.Save();
            if (qwq != null) PlayerPrefs.SetString(dataPath.BaseData.DataTag, qwq);
            SceneTransit.Instance.LoadScene("DSPScene");
        }

        private void SaveNExit()
        {
            broadCastTarget.BroadcastMessage("SaveValue");
            PlayerPrefs.SetString("selected_skin",
                GlobalSetting.CurrentSkinInfo.isExternal
                    ? $"e{GlobalSetting.CurrentSkinInfo.id}"
                    : $"i{(int)GlobalSetting.CurrentSkinInfo.skin}");
            PlayerPrefs.Save();
#if UNITY_IPHONE && !UNITY_EDITOR
            PlayerPrefs.SetString("file_path", Application.persistentDataPath);
            PlayerPrefs.Save();

            if (!Directory.Exists(Util.DataPath))
            {
                InGameUIManager.ShowModalWindowWithClose("故意的是吧", "你这文件夹都不存在啊", () => { }, "确认");
                return;
            }
#endif

            SceneTransit.Instance.Back();
        }

        public void UpdateSelectedSkinItem(bool isExternal, string id)
        {
            if (isExternal && id == "")
            {
                OpenFile.LoadFile(AddSkinFromPackage, () => { },
                    new[] { new ExtensionFilter("Phira皮肤包", "zip") }, null, "选择皮肤包…", "确定");
                return;
            }

            if (selectedIsExternal == isExternal && selectedId == id) return;
            SkinInfo newSkinInfo = HitEffectManager.GetInstance().GetSkinInfo(isExternal, id);
            if (newSkinInfo == null)
            {
                if (!isExternal) throw new ArgumentException();
                SkinManager.Instance.DeleteSkinInfo(id);
                RefreshExternalSkins();
                return;
            }

            selectedIsExternal = isExternal;
            selectedId = id;
            foreach (var internalSkinItem in internalSkinItems.Values)
            {
                internalSkinItem.SetSelected(isExternal, id);
            }

            foreach (var externalSkinItem in externalSkinItems.Values)
            {
                externalSkinItem.SetSelected(isExternal, id);
            }

            GlobalSetting.CurrentSkinInfo = newSkinInfo;
            OnSkinChanged();
        }

        private void OnSkinChanged()
        {
#if UNITY_EDITOR
            hitFx = GlobalSetting.CurrentSkinInfo.hitFx;
#endif
            HitSoundManager.Instance.RefreshHitSounds();
            delayCorrect.OnSkinChanged();
            skinPreviewer.UpdateSkin();
            EffectSystemManager.Instance.UpdateSkin();
        }

        private async void AddSkinFromPackage(string path)
        {
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

        private float tmp;
        public void PlayHitEffect(int type)
        {
            tmp = GlobalSetting.GlobalNoteScale;
            GlobalSetting.GlobalNoteScale = SkinPreview.Size;
            EffectManager hitFxObj = HitEffectManager.GetInstance()
                .GetObj((HitFxJudgeType)type, GlobalSetting.CurrentSkinInfo);
            hitFxObj.transform.position = hitEffectPos.position;
            hitFxObj.transform.rotation = Quaternion.identity;
            hitFxObj.PlayEffect();
            GlobalSetting.GlobalNoteScale = tmp;
        }

        public void Test(string text)
        {
            Debug.Log(text);
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