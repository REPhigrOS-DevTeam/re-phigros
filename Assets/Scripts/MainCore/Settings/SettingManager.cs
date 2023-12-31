using System.Collections.Generic;
using System.IO;
using Lean.Gui;
using MainCore.Common;
using MainCore.UI;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Settings
{
    public class SettingManager : MonoBehaviour
    {
        [SerializeField] private LeanButton saveNExit;
        [SerializeField] private LeanButton dspEnter;
        [SerializeField] private InputField_File_Selector dataPath;
        [SerializeField] private Transform broadCastTarget;
        [SerializeField] private LeanToggle[] toggles;
        [SerializeField] private DelayCorrect delayCorrect;
        [SerializeField] private RectTransform internalSkinParent, externalSkinParent;
        [SerializeField] private GameObject skinItemPrefab;
        [SerializeField] private Button openSkinSelector, closeSkinSelector;
        [SerializeField] private GameObject skinSelectorCanvas, skinPreview;
        [SerializeField] private SkinPreview skinPreviewer;
        private List<SkinItem> internalSkinItems = new List<SkinItem>(), externalSkinItems = new List<SkinItem>();
        private bool selectedIsExternal = false;
        private string selectedId = "-1";

        void Start()
        {
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
            for (int i = 0; i < internalMax; i++)
            {
                GameObject o = Instantiate(skinItemPrefab, internalSkinParent);
                o.name = ((Skin)i).ToString();
                SkinItem skinItem = o.GetComponent<SkinItem>();
                skinItem.Init(this, false, i.ToString(), ((Skin)i).ToString());
                internalSkinItems.Add(skinItem);
            }
            
            RefreshExternalSkins();
            
            internalSkinItems[(int)GlobalSetting.Skin].GetComponent<Button>().onClick.Invoke();
        }

        private void RefreshExternalSkins()
        {
            for (int i = 0; i < externalSkinParent.childCount; i++)
            {
                Destroy(externalSkinParent.GetChild(i).gameObject);
            }
            SkinSummary[] skinSummaries = SkinManager.Instance.GetSkinSummaries();
            foreach (SkinSummary skinSummary in skinSummaries)
            {
                GameObject o = Instantiate(skinItemPrefab, externalSkinParent);
                o.name = skinSummary.id;
                SkinItem skinItem = o.GetComponent<SkinItem>();
                skinItem.Init(this, true, skinSummary.id, skinSummary.name);
                externalSkinItems.Add(skinItem);
            }
            GameObject add = Instantiate(skinItemPrefab, externalSkinParent);
            add.name = "Add";
            SkinItem skinItem1 = add.GetComponent<SkinItem>();
            skinItem1.Init(this, true, "", "＋");
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
            PlayerPrefs.Save();
#if UNITY_IPHONE && !UNITY_EDITOR
            PlayerPrefs.SetString("file_path", Application.persistentDataPath);
            PlayerPrefs.Save();

            if (!Directory.Exists(PlayerPrefs.GetString("file_path", Application.persistentDataPath)))
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
                // TODO: 文件选择并导入
                return;
            }
            if (selectedIsExternal == isExternal && selectedId == id) return;
            selectedIsExternal = isExternal;
            selectedId = id;
            foreach (var internalSkinItem in internalSkinItems)
            {
                internalSkinItem.SetSelected(isExternal, id);
            }

            foreach (var externalSkinItem in externalSkinItems)
            {
                externalSkinItem.SetSelected(isExternal, id);
            }
            OnSkinChanged();
        }

        private void OnSkinChanged()
        {
            GlobalSetting.CurrentSkinInfo = HitEffectManager.GetInstance().GetSkinInfo(selectedIsExternal, selectedId);
            HitSoundManager.Instance.RefreshHitSounds();
            delayCorrect.OnSkinChanged();
            skinPreviewer.UpdateSkin();
        }
        
        private async void AddSkinFromPackage(string path)
        {
            string tmpDirPath = Application.temporaryCachePath + "/tmpSkinPackage";
            string dirPath = $"{tmpDirPath}/{Path.GetFileNameWithoutExtension(path)}";
            if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);
            try
            {
                ZipUtils.UnZip(await File.ReadAllBytesAsync(path), tmpDirPath + $"/{Path.GetFileNameWithoutExtension(path)}");
            }
            catch (IOException)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "无法读取文件", () => { }, "确定");
                return;
            }
            SkinManager.Instance.AddSkinInfo(dirPath, await GameUtils.ReadSkin(dirPath));
        }

        public void PlayHitSound(int id)
        {
            HitSoundManager.Instance.Play(id);
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