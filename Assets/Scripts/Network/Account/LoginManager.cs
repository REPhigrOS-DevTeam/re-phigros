using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Baracuda.Threading;
using Cysharp.Threading.Tasks;
using MainCore.Common;
using MainCore.Utilities;
using Network.Account.Serialized;
using Network.Account.Utils;
using Network.API;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Network.Account
{
    public class LoginManager : MonoBehaviour
    {
        public InputField ifUsername, ifPassword;
        public Toggle tRememberMe;
        public Button bLogin;
        private static readonly Regex Regex = new Regex("[^a-zA-Z0-9]");
        private string username = "", password = "";
        private bool firstLogin = true;
        public GameObject loginMask;
        private bool lastRememberMe;

        public static bool RememberMe;
        public static string Username = "";
        public static string VerifyToken = "";

        private void Awake()
        {
            ifUsername.onEndEdit.AddListener(CheckUsername);
            ifPassword.onEndEdit.AddListener(CheckPassword);
            bLogin.onClick.AddListener(Login);
            lastRememberMe = tRememberMe.isOn = RememberMe;
            if (RememberMe)
            {
                ifUsername.text = username = Username;
                ifPassword.text = "0000000000000000";
            }

            tRememberMe.onValueChanged.AddListener(OnInputFieldsClicked);
            AddInputFieldClickEvent(ifUsername, () => { OnInputFieldsClicked(false); });
            AddInputFieldClickEvent(ifPassword, () => { OnInputFieldsClicked(false); });
            loginMask.SetActive(false);
        }

        private void OnInputFieldsClicked(bool a)
        {
            if (!RememberMe) return;
            Username = VerifyToken = "";
            firstLogin = false;
            ifPassword.text = password = "";
            lastRememberMe = false;
        }

        private void CheckUsername(string input)
        {
            if (Regex.IsMatch(input))
            {
                ifUsername.text = username;
            }
            else
            {
                username = ifUsername.text;
            }
        }

        private void CheckPassword(string input)
        {
            if (Regex.IsMatch(input))
            {
                ifPassword.text = password;
            }
            else
            {
                password = ifPassword.text;
            }
        }

        private void AddInputFieldClickEvent(InputField inputField, Action selectEvent) //可以在Awake中调用
        {
            var eventTrigger = inputField.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = inputField.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry onClick = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select
            };

            onClick.callback.AddListener(_ => { selectEvent.Invoke(); });
            eventTrigger.triggers.Add(onClick);
        }

        private async void Login()
        {
            if (ifUsername.text.Length == 0 || ifPassword.text.Length == 0)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "用户名或密码为空", () => { }, "确定");
                return;
            }

            if (ifUsername.text.Length > 30)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "用户名过长", () => { }, "确定");
                return;
            }

            RememberMe = tRememberMe.isOn;
            SaveRememberMe();
            loginMask.SetActive(true);
            await UniTask.SwitchToMainThread();
            if (lastRememberMe && RememberMe)
            {
                StatusCode code = await Verify();
                loginMask.SetActive(false);
                switch (code)
                {
                    case StatusCode.Unknown:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了未定义的状态码，请联系开发者\n程序即将退出", Util.QuitApp,
                            "确定");
                        break;
                    case StatusCode.OK:
                        InGameUIManager.ShowModalWindowWithClose("提示", "登录成功", OnLoginSucceeded, "确定");
                        break;
                    case StatusCode.InvalidParam:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了不应出现的状态码（上传了非法参数），请联系开发者\n程序即将退出",
                            Util.QuitApp, "确定");
                        break;
                    case StatusCode.ServerInternalError:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "服务器出错，请联系开发者\n程序即将退出", Util.QuitApp, "确定");
                        break;
                    case StatusCode.IllegalLogin:
                        InGameUIManager.ShowModalWindowWithClose("错误", "登录次数已用完", () => { }, "确定");
                        break;
                    case StatusCode.InvalidUsername:
                        InGameUIManager.ShowModalWindowWithClose("错误", "用户名不合法（但是已经登录过了为啥报这个）", () => { }, "确定");
                        break;
                    case StatusCode.InvalidToken:
                        InGameUIManager.ShowModalWindowWithClose("错误", "Token无效，请重新登录", () => { }, "确定");
                        break;
                    case StatusCode.NoPermission:
                        InGameUIManager.ShowModalWindowWithClose("错误", "没有内测权限", () => { }, "确定");
                        break;
                    case StatusCode.InvalidPassword:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                StatusCode code = await Login(username, password);
                loginMask.SetActive(false);
                switch (code)
                {
                    case StatusCode.Unknown:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了未定义的状态码，请联系开发者\n程序即将退出", Util.QuitApp,
                            "确定");
                        break;
                    case StatusCode.OK:
                        InGameUIManager.ShowModalWindowWithClose("提示", "登录成功", OnLoginSucceeded, "确定");
                        break;
                    case StatusCode.InvalidParam:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了不应出现的状态码（上传了非法参数），请联系开发者\n程序即将退出",
                            Util.QuitApp, "确定");
                        break;
                    case StatusCode.ServerInternalError:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "服务器出错，请联系开发者\n程序即将退出", Util.QuitApp,
                            "确定");
                        break;
                    case StatusCode.IllegalLogin:
                        InGameUIManager.ShowModalWindowWithClose("错误", "登录次数已用完", () => { }, "确定");
                        break;
                    case StatusCode.InvalidUsername:
                        InGameUIManager.ShowModalWindowWithClose("错误", "用户名不合法", () => { }, "确定");
                        break;
                    case StatusCode.InvalidPassword:
                        InGameUIManager.ShowModalWindowWithClose("错误", "密码错误", () => { }, "确定");
                        break;
                    case StatusCode.NoPermission:
                        InGameUIManager.ShowModalWindowWithClose("错误", "没有内测权限", () => { }, "确定");
                        break;
                    case StatusCode.InvalidToken:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private bool qwq;

        private void OnLoginSucceeded()
        {
            if (qwq) return;
            qwq = true;
            if (RememberMe) SaveUsernameAndToken();
            if (!PlayerPrefs.HasKey("first_start"))
            {
                PlayerPrefs.SetInt("first_start", 1);
                PlayerPrefs.Save();
                SceneTransit.Instance.TransitTo("SettingsScene");
            }
            else
            {
                SceneTransit.Instance.TransitTo("ChartSelectorScene");
            }
        }

        public static async Task<StatusCode> Login(string username, string password)
        {
#if !UNITY_EDITOR
            Debug.Log("Try login");
#endif
            var builder = new UriBuilder(RepAPI.APIBase.UrlCombine(RepAPI.loginUrl))
            {
                Query = $"username={username}&password={password}"
            };
            string uri = builder.Uri.ToString();
#if UNITY_EDITOR
            Debug.Log("Try send for login: " + uri);
#endif
            byte[] data = await uri.SendGetRequestAsync();
            if (data == null)
            {
                Debug.Log("RePhigros API: Unable to connect to server when logging");
                return StatusCode.Unknown;
            }

            string result = Encoding.UTF8.GetString(data);
            var res = JsonConvert.DeserializeObject<VerifyRequest>(result);
            if (res == null || res.status == false)
            {
                Debug.LogError($"RePhigros API: Error logging in, with code {(int)(res?.Code ?? StatusCode.Unknown)}");
                return res?.Code ?? StatusCode.Unknown;
            }

            if (res.Code != StatusCode.OK) throw new ArgumentException("吃席");
            Debug.Log($"RePhigros API: Successfully logged in with verifyToken: {ProtectToken(res.verifyToken)}");
            Username = username;
            VerifyToken = res.verifyToken;
            return StatusCode.OK;
        }

        public static async Task<StatusCode> Verify()
        {
#if !UNITY_EDITOR
            Debug.Log("Try verify");
#endif
            if (Username == "" || VerifyToken == "")
            {
                Debug.LogError("RePhigros API: Undefined behaviour detected, trying to verify without login.");
            }

            var builder = new UriBuilder(RepAPI.APIBase.UrlCombine(RepAPI.verifyUrl))
            {
                Query = $"username={Username}&verifytoken={VerifyToken}"
            };
            string uri = builder.Uri.ToString();
#if UNITY_EDITOR
            Debug.Log("Try send for verify: " + uri);
#endif
            byte[] data = await uri.SendGetRequestAsync();
            if (data == null)
            {
                Debug.LogError($"RePhigros API: Unable to connect to server when verifying");
                return StatusCode.Unknown;
            }

            string result = Encoding.UTF8.GetString(data);
            var res = JsonConvert.DeserializeObject<VerifyRequest>(result);
            if (res == null || res.status == false)
            {
                Debug.LogError($"RePhigros API: Error verifying, with code {(int)(res?.Code ?? StatusCode.Unknown)}");
                return res?.Code ?? StatusCode.Unknown;
            }

            if (res.Code != StatusCode.OK) throw new ArgumentException("吃席");
            Debug.Log($"RePhigros API: Access granted");
            VerifyToken = res.verifyToken;
            return StatusCode.OK;
        }

        // public static bool IsLoggedIn()
        // {
        //     return Username != "" && VerifyToken != "";
        // }

        public static void SaveRememberMe()
        {
            PlayerPrefsExtension.SetBoolean("repapi_rememberme", RememberMe);
            PlayerPrefs.Save();
        }

        public static void SaveUsernameAndToken()
        {
            PlayerPrefs.SetString("repapi_playername", Username);
            PlayerPrefs.SetString("repapi_verifytoken", VerifyToken);
            PlayerPrefs.Save();
        }

        public static void ResetUsernameAndToken()
        {
            Username = "";
            VerifyToken = "";
            SaveUsernameAndToken();
        }

        private static string ProtectToken(string token)
        {
            return token.Substring(0, 7) + string.Concat(Enumerable.Repeat("*", token.Length - 14)) +
                   token.Substring(token.Length - 7);
        }

        public static void ReadAccountFromPlayerPrefs(bool refresh = false)
        {
            RememberMe = PlayerPrefsExtension.GetBoolean("repapi_rememberme", false);
            if (RememberMe)
            {
                Username = PlayerPrefs.GetString("repapi_playername", "");
                VerifyToken = PlayerPrefs.GetString("repapi_verifytoken", "");
            }
            else if (refresh)
            {
                Username = "";
            }
        }
    }
}