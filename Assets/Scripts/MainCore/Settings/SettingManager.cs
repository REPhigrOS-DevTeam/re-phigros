using System.IO;
using Lean.Gui;
using MainCore.Common;
using UnityEngine;

namespace MainCore.Settings
{
    public class SettingManager : MonoBehaviour
    {
        [SerializeField] private LeanButton saveNExit;
        [SerializeField] private LeanButton dspEnter;
        [SerializeField] private InputField_String_Setting dataPath;
        [SerializeField] private Transform broadCastTarget;
        [SerializeField] private LeanToggle[] toggles;

        void Start()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            dataPath.gameObject.SetActive(false);
#else
            if (!PlayerPrefs.HasKey("file_path"))
            {
                dataPath.SetValue($"{Application.persistentDataPath}");
            }
#endif
            saveNExit.OnClick.AddListener(SaveNExit);
            dspEnter.OnClick.AddListener(() => { SceneTransit.Instance.TransitTo("DSPScene"); });
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
            SpecialEvent qiYongExtraJson = new SpecialEvent(toggles, new[] { 5, 6, 4, 8 },
                () =>
                {
                    // Debug.Log("启用Shader");
                    // InGameUIManager.ShowModalWindowWithClose("<size=15>提示</size>", "Extra.json已启用", () => { },
                    //     "确认");
                    // GlobalSetting.useShader = true;
                });
        }

        private void SaveNExit()
        {
            broadCastTarget.BroadcastMessage("SaveValue");
            PlayerPrefs.Save();
#if UNITY_IPHONE && !UNITY_EDITOR
            PlayerPrefs.SetString("file_path", Application.persistentDataPath);
            PlayerPrefs.Save();
#endif
            if (!Directory.Exists(PlayerPrefs.GetString("file_path", Application.persistentDataPath)))
            {
                InGameUIManager.ShowModalWindowWithClose("故意的是吧", "你这文件夹都不存在啊", () => { }, "确认");
                return;
            }

            SceneTransit.Instance.TransitTo("ChartSelectorScene");
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