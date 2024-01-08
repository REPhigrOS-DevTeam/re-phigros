using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using MainCore.Common;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace MainCore.UI
{
    public class EntryManager : MonoBehaviour
    {
        // [SerializeField] private Button touchToStart;
        // [SerializeField] private Text touchToStartText;

        private bool temp = false;

        public void Start()
        {
            ZipConstants.DefaultCodePage = 65001; // UTF-8
            Update();
        }

        void Update()
        {
            if (Time.timeSinceLevelLoad < 1 || !Input.GetMouseButtonUp(0) || temp) return;
            GlobalSetting.OriginResolution = Screen.currentResolution;
            LoadIn();
            // StartCoroutine(CountDown());
            temp = true;
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

        private void LoadIn()
        {
//#if !UNITY_EDITOR
            Application.targetFrameRate = 120;
            GameUtils.ResetDSPBuffer(PlayerPrefs.GetInt("dsp_pow", 8));
            if (!File.Exists(Path.Combine(Application.persistentDataPath, "FuckIOS（别删）")))
            {
                var t = File.Create(Path.Combine(Application.persistentDataPath, "FuckIOS（别删）"));
                t.Dispose();
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "FuckIOS（别删）"), "Fucking IOS...");
            }
//#endif

            if (!PlayerPrefs.HasKey("first_start"))
            {
                PlayerPrefs.SetInt("first_start", 1);
                SceneTransit.Instance.TransitTo("SettingsScene");
            }
            else
            {
                SceneTransit.Instance.TransitTo("ChartSelectorScene");
            }
        }
    }
}