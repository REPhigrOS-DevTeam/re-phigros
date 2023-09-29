using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ICSharpCode.SharpZipLib.Zip;
using MainCore.Common;
using MainCore.Utilities;
using Network;
using Network.Account;
using Network.Multiplayer.Managers;
using Newtonsoft.Json;
using UnityEngine;
using Utilities;

namespace MainCore.UI
{
    public class EntryManager : MonoBehaviour
    {
        // [SerializeField] private Button touchToStart;
        // [SerializeField] private Text touchToStartText;

        private bool clicked = false;

        private void Awake()
        {
            ZipConstants.DefaultCodePage = 65001; // UTF-8
            SceneTransit.OnSceneClosing += () => HitEffectManager.GetInstance().Reset();
            SocketManager.Init(); 
        }

        private async Task<bool> InitAPI()
        {
            PopupMessageManager.Instance.Message("尝试连接服务器……");
            LoginManager.ReadAccountFromPlayerPrefs();
            bool succeeded = await RepAPI.Init();
            if (!succeeded)
            {
                InGameUIManager.ShowModalWindowWithClose("致命错误", "无法连接至服务器\n程序即将退出", Util.QuitApp, "确定");
            }
            else
            {
                PopupMessageManager.Instance.Message("连接成功");
            }

            return succeeded;
        }

        private void Start()
        {
            Update();
        }

        private void Update()
        {
            if (clicked || Time.timeSinceLevelLoad < 1 || !Input.GetMouseButtonUp(0)) return;
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
            Application.targetFrameRate = 120;
            GameUtils.ResetDSPBuffer(PlayerPrefs.GetInt("dsp_pow", 8));
            if (!File.Exists(Path.Combine(Application.persistentDataPath, "FuckIOS（别删）")))
            {
                var t = File.Create(Path.Combine(Application.persistentDataPath, "FuckIOS（别删）"));
                await t.DisposeAsync();
                await File.WriteAllTextAsync(Path.Combine(Application.persistentDataPath, "FuckIOS（别删）"),
                    "Fucking IOS...");
            }

//#endif
            if (await InitAPI()) SceneTransit.Instance.JumpScene("LoginScene");
        }
    }
}