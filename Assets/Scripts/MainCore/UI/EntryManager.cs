using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using MainCore.Common;
using MainCore.Utilities;
using Network.Verify.API;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace MainCore.UI
{
    public class EntryManager : MonoBehaviour
    {
        // [SerializeField] private Button touchToStart;
        // [SerializeField] private Text touchToStartText;

        private bool clicked = false;
        private bool apiPrepared = false;

        private void Awake()
        {
            ZipConstants.DefaultCodePage = 65001; // UTF-8
        }

        private async void InitAPI()
        {
            PopupMessageManager.Instance.Message("尝试连接服务器……");
            bool succeeded = await RepAPI.Init();
            if (!succeeded)
            {
                InGameUIManager.ShowModalWindowWithClose("致命错误", "无法连接至服务器\n程序即将退出", Util.QuitApp, "确定");
            }
            else
            {
                apiPrepared = true;
                PopupMessageManager.Instance.Message("连接成功");
            }
        }

        private void Start()
        {
            InitAPI();
            Update();
        }

        private void Update()
        {
            if (clicked || Time.timeSinceLevelLoad < 1 || !Input.GetMouseButtonUp(0) || !apiPrepared) return;
            GlobalSetting.OriginResolution = Screen.currentResolution;
            LoadIn();
            // StartCoroutine(CountDown());
            clicked = true;
        }

        // private IEnumerator CountDown()
        // {
        //     //InGameUIManager.ShowModalWindowWithClose("测试", "这是一个测试罢了", () => {}, "确认");
        //     touchToStart.onClick.AddListener(LoadIn);
        //     //await Task.Delay(1000);
        //     touchToStartText.text = "\n\n\n\n\n\n3";
        //     yield return new WaitForSeconds(1f);
        //     //await Task.Delay(1000);
        //     touchToStartText.text = "\n\n\n\n\n\n2";
        //     yield return new WaitForSeconds(1f);
        //     //await Task.Delay(1000);
        //     touchToStartText.text = "\n\n\n\n\n\n1";
        //     touchToStartText.DOFade(0, 1f);
        //     yield return new WaitForSeconds(1f);
        //     //await Task.Delay(1000);
        //     touchToStartText.DOFade(1, 1f);
        //     touchToStartText.text = "\n\n\n\n\n\nTap to continue";
        //     GlobalSetting.OriginResolution = Screen.currentResolution;
        //     touchToStart.interactable = true;
        // }

        private async void LoadIn()
        {
            if (PlayerPrefs.GetInt("anomaly_mode", 0) == 1)
            {
                NeedUpdate();
                return;
            }

            bool isInRange = await TimeBomb.IsInRange(new DateTime(2023, 9, 28, 0, 0, 0, DateTimeKind.Utc), true);
            if (!isInRange)
            {
                PlayerPrefs.SetInt("anomaly_mode", 1);
                PlayerPrefs.Save();
                NeedUpdate();
                return;
            }

            Application.targetFrameRate = 120;
            GameUtils.ResetDSPBuffer(PlayerPrefs.GetInt("dsp_pow", 8));
            if (!File.Exists(Path.Combine(Application.persistentDataPath, "FuckIOS（别删）")))
            {
                var t = File.Create(Path.Combine(Application.persistentDataPath, "FuckIOS（别删）"));
                await t.DisposeAsync();
                await File.WriteAllTextAsync(Path.Combine(Application.persistentDataPath, "FuckIOS（别删）"), "Fucking IOS...");
            }

//#endif
            SceneTransit.Instance.TransitTo("LoginScene");
        }

        private void NeedUpdate()
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "请升级到最新版", () =>
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }, "退出");
        }
    }
}