using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using LeanCloud;
using LeanCloud.Storage;
using MainCore.Common;
using MainCore.Utilities;
using Network;
using Network.Account;
using Network.Account.Utils;
using Network.Multiplayer.Managers;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

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
            GlobalSetting.UnityThreadId = Thread.CurrentThread.ManagedThreadId;
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
            LCApplication.Initialize("iIOds0wITw7kHcLEX6u39Moo-gzGzoHsz", "sn3gIRQAP47rSCI3GvcxUJpl",
                "https://iiods0wi.lc-cn-n1-shared.com");
            SocketManager.Init();
            LCLogger.LogDelegate = (level, info) =>
            {
                switch (level)
                {
                    case LCLogLevel.Debug:
                        Debug.Log($"[DEBUG] {DateTime.Now} {info}\n");
                        break;
                    case LCLogLevel.Warn:
                        Debug.Log($"[WARNING] {DateTime.Now} {info}\n");
                        break;
                    case LCLogLevel.Error:
                        Debug.Log($"[ERROR] {DateTime.Now} {info}\n");
                        break;
                    default:
                        Debug.Log(info);
                        break;
                }
            };
            // splashCanvas.SetActive(true);
            // splashBgCanvas.SetActive(true);
            // splashPlayer.loopPointReached += async _ =>
            // {
            //     splashCanvas.SetActive(false);
            //     splashBgCanvas.SetActive(false);
            //     await UniTask.Yield();
            //     inited = true;
            // };
            // splashPlayer.prepareCompleted += _ => prepared = true;
            // splashPlayer.Prepare();
        }

        // private async UniTask<bool> InitAPI()
        // {
        //     PopupMessageManager.Instance.Message("尝试连接服务器……");
        //     // LoginManagerOld.ReadAccountFromPlayerPrefs();
        //     bool succeeded = await RepAPI.Init();
        //     if (!succeeded)
        //     {
        //         Debug.Log("我是扇贝");
        //         InGameUIManager.ShowModalWindowWithClose("致命错误", "无法连接至服务器\n程序即将退出", Util.QuitApp, "确定");
        //     }
        //     else
        //     {
        //         PopupMessageManager.Instance.Message("连接成功");
        //     }
        //
        //     return succeeded;
        // }

        private void Start()
        {
            // InitVideo(cts.Token);
            // Update();
            LoadIn();
        }

        // private async void InitVideo(CancellationToken cancellationToken)
        // {
        //     await UniTask.WaitUntil(() => prepared, cancellationToken: cancellationToken);
        //     splashPlayer.time = 0f;
        //     splashPlayer.Play();
        // }
        //
        private void Update()
        {
            // if (Input.GetMouseButtonUp(0) && !inited)
            // {
            //     if (!splashPlayer.isPlaying && !splashPlayer.isPrepared)
            //     {
            //         cts.Cancel();
            //     }
            //
            //     splashPlayer.Stop();
            //     splashCanvas.SetActive(false);
            //     splashBgCanvas.SetActive(false);
            //     inited = true;
            //     return;
            // }
            //
            // if (clicked || !inited || !Input.GetMouseButtonUp(0)) return;
            // LoadIn();
            // // StartCoroutine(CountDown());
            // clicked = true;
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
            await new WaitUntil(() => SkinManager.Instance.Inited);
            GlobalSetting.ReadUserSettings();
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
            bool loginCompleted = false;
#if UNITY_EDITOR
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            await UniTask.SwitchToMainThread();
            PopupMessageManager.Instance.Message("尝试连接服务器...");
            await UniTask.SwitchToThreadPool();
            if (await RepAPI.Init())
            {
                await UniTask.SwitchToMainThread();
                if (AccountManager.GetLastUser() == "")
                {
                    PopupMessageManager.Instance.ChangeContent("连接成功");
                    loginCompleted = true;
                }
                else
                {
                    PopupMessageManager.Instance.ChangeContent("尝试登录...");
                    List<AccountManager.AccountInfo> accountInfos = AccountManager.GetAccountList();
                    int a = -1;
                    AccountManager.AccountInfo info = null;
                    for (var i = 0; i < accountInfos.Count; i++)
                    {
                        if (accountInfos[i].Username == AccountManager.GetLastUser())
                        {
                            a = i;
                            info = accountInfos[i];
                            break;
                        }
                    }

                    if (info == null) throw new ArgumentException();
                    await UniTask.SwitchToThreadPool();
                    (StatusCode code, string? token) = await LoginManager.Verify(info);
                    await UniTask.SwitchToMainThread();
                    switch (code)
                    {
                        case StatusCode.Unknown:
                            InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了未定义的状态码，请联系开发者\n程序即将退出", Util.QuitApp,
                                "确定");
                            break;
                        case StatusCode.OK:
                            accountInfos.Remove(info);
                            info.VerifyToken = token;
                            accountInfos.Insert(0, info);
                            AccountManager.SaveAccountList(accountInfos, info.Username);
                            Finally(info);
                            PopupMessageManager.Instance.ChangeContent("登录成功");
                            loginCompleted = true;
                            break;
                        case StatusCode.InvalidParam:
                            InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了不应出现的状态码：非法参数，请联系开发者\n程序即将退出",
                                Util.QuitApp, "确定");
                            break;
                        case StatusCode.ServerInternalError:
                            InGameUIManager.ShowModalWindowWithClose("致命错误", "服务器出错，请联系开发者\n程序即将退出", Util.QuitApp,
                                "确定");
                            break;
                        case StatusCode.IllegalLogin:
                            InGameUIManager.ShowModalWindowWithClose("错误", "登录次数已用完", () => loginCompleted = true,
                                "确定");
                            break;
                        case StatusCode.InvalidUsername:
                            InGameUIManager.ShowModalWindowWithClose("错误", "用户名不合法（但是已经登录过了为啥报这个）",
                                () => loginCompleted = true, "确定");
                            break;
                        case StatusCode.InvalidToken:
                            accountInfos[a].VerifyToken = "";
                            AccountManager.SaveAccountList(accountInfos, info.Username);
                            accountInfos.Remove(info);
                            PopupMessageManager.Instance.ChangeContent("Token无效，请重新登录");
                            loginCompleted = true;
                            break;
                        case StatusCode.NoPermission:
                            InGameUIManager.ShowModalWindowWithClose("错误", "没有内测权限", () => loginCompleted = true, "确定");
                            break;
                        case StatusCode.InvalidPassword:
                            InGameUIManager.ShowModalWindowWithClose("致命错误", "调用验证接口时收到了不应出现的状态码：密码不合法\n程序即将退出",
                                Util.QuitApp,
                                "确定");
                            break;
                        case StatusCode.UserBanned:
                            InGameUIManager.ShowModalWindowWithClose("悲报", "您已被封禁", () => loginCompleted = true, "确定");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                await UniTask.SwitchToMainThread();
                PopupMessageManager.Instance.ChangeContent("错误：无法连接服务器");
                loginCompleted = true;
            }
#if UNITY_EDITOR
            long milliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            Debug.Log(milliseconds / 1000f);
#endif
            await new WaitUntil(() => loginCompleted);
            if (!PlayerPrefs.HasKey("first_start"))
            {
                PlayerPrefs.SetInt("first_start", 1);
                PlayerPrefs.Save();
                SceneTransit.Instance.AppendScene("MainScene");
                SceneTransit.Instance.AppendScene("SettingsScene");
                SceneTransit.Instance.LoadScene("DSPScene", 0);
            }
            else
            {
                SceneTransit.Instance.JumpScene("MainScene", 0);
            }
            //if (await InitAPI()) SceneTransit.Instance.JumpScene("LoginScene");
        }

        private void Finally(AccountManager.AccountInfo info)
        {
            GlobalSetting.Username = info.Username;
            GlobalSetting.VerifyToken = info.VerifyToken;
        }
    }
}