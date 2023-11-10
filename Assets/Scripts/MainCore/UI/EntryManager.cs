using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using MainCore.Common;
using MainCore.Utilities;
using Network;
using Network.Multiplayer.Managers;
using UnityEngine;
using UnityEngine.Video;

namespace MainCore.UI
{
    public class EntryManager : MonoBehaviour
    {
        // [SerializeField] private Button touchToStart;
        // [SerializeField] private Text touchToStartText;
        [SerializeField] private GameObject debugText;
        [SerializeField] private GameObject inGameDebugConsolePrefab;
        [SerializeField] private VideoPlayer splashPlayer;
        [SerializeField] private GameObject splashCanvas, splashBgCanvas;
        
        private bool clicked = false;
        private bool prepared = false;
        private bool inited = false;
        
        private CancellationTokenSource cts = new CancellationTokenSource();

        private void Awake()
        {
            GlobalSetting.OriginResolution = Screen.currentResolution;
#if !RELEASE_VERSION && !UNITY_EDITOR
            debugText.SetActive(true);
            Instantiate(inGameDebugConsolePrefab).GetComponent<DebugLogManager>().enableCommand = false;
#else
            debugText.SetActive(false);
#if UNITY_EDITOR
            // 这个是用来消除Rider的代码提示的
            if (false) Instantiate(inGameDebugConsolePrefab).GetComponent<DebugLogManager>();
#endif
#endif
            SceneTransit.OnSceneClosing += () => HitEffectManager.GetInstance().Reset();
            SocketManager.Init();
            splashCanvas.SetActive(true);
            splashBgCanvas.SetActive(true);
            splashPlayer.loopPointReached += async _ =>
            {
                splashCanvas.SetActive(false);
                splashBgCanvas.SetActive(false);
                await UniTask.Yield();
                inited = true;
            };
            splashPlayer.prepareCompleted += _ => prepared = true;
            splashPlayer.Prepare();
        }

        private async UniTask<bool> InitAPI()
        {
            PopupMessageManager.Instance.Message("尝试连接服务器……");
            // LoginManagerOld.ReadAccountFromPlayerPrefs();
            bool succeeded = await RepAPI.Init();
            if (!succeeded)
            {
                Debug.Log("我是扇贝");
                // InGameUIManager.ShowModalWindowWithClose("致命错误", "无法连接至服务器\n程序即将退出", Util.QuitApp, "确定");
            }
            else
            {
                PopupMessageManager.Instance.Message("连接成功");
            }

            return succeeded;
        }

        private void Start()
        {
            InitVideo(cts.Token);
            Update();
        }

        private async void InitVideo(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => prepared, cancellationToken:cancellationToken);
            splashPlayer.time = 0f;
            splashPlayer.Play();
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0) && !inited)
            {
                if (!splashPlayer.isPlaying && !splashPlayer.isPrepared)
                {
                    cts.Cancel();
                }
                splashPlayer.Stop();
                splashCanvas.SetActive(false);
                splashBgCanvas.SetActive(false);
                inited = true;
                return;
            }
            if (clicked || !inited || !Input.GetMouseButtonUp(0)) return;
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
#if !RELEASE_VERSION && !UNITY_EDITOR
            GlobalSetting.username = "development";
            GlobalSetting.verifyToken = "";
            if (!PlayerPrefs.HasKey("first_start"))
            {
                PlayerPrefs.SetInt("first_start", 1);
                PlayerPrefs.Save();
                SceneTransit.Instance.AppendScene("MainScene");
                SceneTransit.Instance.AppendScene("SettingsScene");
                SceneTransit.Instance.LoadScene("DSPScene");
            }
            else
            {
                SceneTransit.Instance.JumpScene("MainScene");
            }
#else
            if (await InitAPI()) SceneTransit.Instance.JumpScene("LoginScene");
#endif
        }
    }
}