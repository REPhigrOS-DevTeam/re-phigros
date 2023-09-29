using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MainCore;
using MainCore.Common;
using MainCore.Utilities;
using Network.Account.Serialized;
using Network.Account.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

// verify token是128位随机字符串
namespace Network.Account
{
    public class LoginManager : MonoBehaviour
    {
        private List<AccountManager.AccountInfo> accountInfos;
        [SerializeField] private InputField usernameInputField, passwordInputField;
        [SerializeField] private Dropdown accountsDropdown;
        [SerializeField] private Button loginButton;
        [SerializeField] private Toggle_Button createButton;
        [SerializeField] private GameObject loginMask;
        private bool isCreate;

        private void Awake()
        {
            createButton.onOnLabel = "返回";
            createButton.onOffLabel = "新建...";
            createButton.OnValueChanged += b =>
            {
                if (b) Create();
                else UseSave();
            };
            usernameInputField.onValueChanged.AddListener(CheckUsername);
            passwordInputField.onValueChanged.AddListener(CheckPassword);
            loginButton.onClick.AddListener(OnLogin);
            accountInfos = AccountManager.GetAccountList();
            usernameInputField.gameObject.SetActive(false);
            passwordInputField.gameObject.SetActive(false);
            createButton.IsOn = false;
            if (PlayerPrefs.HasKey("repapi_playername") && accountInfos.Count == 0)
            {
                accountInfos.Add(new AccountManager.AccountInfo(PlayerPrefs.GetString("repapi_playername", ""),
                    PlayerPrefs.GetString("repapi_verifytoken", "")));
                AccountManager.SaveAccountList(accountInfos);
                PlayerPrefs.DeleteKey("repapi_playername");
                PlayerPrefs.DeleteKey("repapi_verifytoken");
                PlayerPrefs.Save();
            }

            GenerateDropdown();
            if (accountInfos.Count == 0) Create();
        }


        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void GenerateDropdown()
        {
            accountsDropdown.options.Clear();
            accountsDropdown.AddOptions(accountInfos.Select(x => x.Username).ToList());
        }

        private void UseSave()
        {
            accountsDropdown.gameObject.SetActive(true);
            usernameInputField.gameObject.SetActive(false);
            passwordInputField.gameObject.SetActive(false);
            isCreate = false;
        }

        private void Create()
        {
            accountsDropdown.gameObject.SetActive(false);
            usernameInputField.gameObject.SetActive(true);
            passwordInputField.gameObject.SetActive(true);
            isCreate = true;
        }

        private async void OnLogin()
        {
            await UniTask.SwitchToMainThread();
            if (isCreate)
            {
                if (accountInfos.Where(accountInfo => accountInfo.Username == username).ToArray()
                        .Length > 0)
                {
                    InGameUIManager.ShowModalWindowWithClose("错误", "用户名已经存在", () => { }, "确定");
                    return;
                }

                if (username.Length == 0 || password.Length == 0)
                {
                    InGameUIManager.ShowModalWindowWithClose("错误", "用户名或密码为空", () => { }, "确定");
                    return;
                }

                if (username.Length > 30)
                {
                    InGameUIManager.ShowModalWindowWithClose("错误", "用户名过长", () => { }, "确定");
                    return;
                }

                (StatusCode code, AccountManager.AccountInfo accountInfo) =
                    await Login(username, password);
                loginMask.SetActive(false);
                switch (code)
                {
                    case StatusCode.Unknown:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了未定义的状态码，请联系开发者\n程序即将退出", Util.QuitApp,
                            "确定");
                        break;
                    case StatusCode.OK:
                        InGameUIManager.ShowModalWindowWithClose("提示", "登录成功", () =>
                        {
                            accountInfos.Insert(0, accountInfo);
                            AccountManager.SaveAccountList(accountInfos);
                            Finally(accountInfo);
                        }, "确定");
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
                        InGameUIManager.ShowModalWindowWithClose("错误", "登录次数已用完", () => { }, "确定");
                        break;
                    case StatusCode.InvalidUsername:
                        InGameUIManager.ShowModalWindowWithClose("错误", "用户名不合法", () => { }, "确定");
                        break;
                    case StatusCode.InvalidPassword:
                        InGameUIManager.ShowModalWindowWithClose("错误", "密码错误", () => { passwordInputField.text = ""; }, "确定");
                        break;
                    case StatusCode.NoPermission:
                        InGameUIManager.ShowModalWindowWithClose("错误", "没有内测权限", () => { }, "确定");
                        break;
                    case StatusCode.InvalidToken:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "调用登录接口时收到了不应出现的状态码：非法Token\n程序即将退出",
                            Util.QuitApp,
                            "确定");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                var i = accountsDropdown.value;
                AccountManager.AccountInfo info = accountInfos[i];

                (StatusCode code, string? token) = await Verify(info);
                loginMask.SetActive(false);
                switch (code)
                {
                    case StatusCode.Unknown:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了未定义的状态码，请联系开发者\n程序即将退出", Util.QuitApp,
                            "确定");
                        break;
                    case StatusCode.OK:
                        InGameUIManager.ShowModalWindowWithClose("提示", "登录成功", () =>
                        {
                            accountInfos.Remove(info);
                            info.VerifyToken = token;
                            accountInfos.Insert(0, info);
                            AccountManager.SaveAccountList(accountInfos);
                            Finally(info);
                        }, "确定");
                        break;
                    case StatusCode.InvalidParam:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了不应出现的状态码：非法参数，请联系开发者\n程序即将退出",
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
                        InGameUIManager.ShowModalWindowWithClose("错误", "Token无效，请重新登录", () =>
                        {
                            accountInfos[i].VerifyToken = "";
                            AccountManager.SaveAccountList(accountInfos);
                            accountInfos.Remove(info);
                            usernameInputField.text = info.Username;
                            Create();
                        }, "确定");
                        break;
                    case StatusCode.NoPermission:
                        InGameUIManager.ShowModalWindowWithClose("错误", "没有内测权限", () => { }, "确定");
                        break;
                    case StatusCode.InvalidPassword:
                        InGameUIManager.ShowModalWindowWithClose("致命错误", "调用验证接口时收到了不应出现的状态码：密码不合法\n程序即将退出",
                            Util.QuitApp,
                            "确定");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        private void Finally(AccountManager.AccountInfo info)
        {
            GlobalSetting.username = info.Username;
            GlobalSetting.verifyToken = info.VerifyToken;
            if (!PlayerPrefs.HasKey("first_start"))
            {
                PlayerPrefs.SetInt("first_start", 1);
                PlayerPrefs.Save();
                SceneTransit.Instance.AppendScene("MainScene");
                SceneTransit.Instance.LoadScene("SettingsScene");
            }
            else
            {
                SceneTransit.Instance.JumpScene("MainScene");
            }
        }

        public static async Task<(StatusCode, AccountManager.AccountInfo)> Login(string username, string password)
        {
#if !UNITY_EDITOR
            Debug.Log("Try login");
#endif
            var builder = new UriBuilder(RepAPI.GetAPIBase().UrlCombine(RepAPI.loginUrl))
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
                return (StatusCode.Unknown, null);
            }

            string result = Encoding.UTF8.GetString(data);
            var res = JsonConvert.DeserializeObject<VerifyRequest>(result);
            if (res == null || res.status == false)
            {
                Debug.LogError($"RePhigros API: Error logging in, with code {(int)(res?.Code ?? StatusCode.Unknown)}");
                return (res?.Code ?? StatusCode.Unknown, null);
            }

            if (res.Code != StatusCode.OK) throw new ArgumentException("吃席");
            Debug.Log($"RePhigros API: Successfully logged in with verifyToken: {ProtectToken(res.verifyToken)}");
            return (StatusCode.OK, new AccountManager.AccountInfo(username, res.verifyToken));
        }

        public static async Task<(StatusCode, string?)> Verify(AccountManager.AccountInfo accountInfo)
        {
#if !UNITY_EDITOR
            Debug.Log("Try verify");
#endif
            if (accountInfo.Username == "" || accountInfo.VerifyToken == "")
            {
                Debug.LogError("RePhigros API: Undefined behaviour detected, trying to verify without login.");
            }

            var builder = new UriBuilder(RepAPI.GetAPIBase().UrlCombine(RepAPI.verifyUrl))
            {
                Query = $"username={accountInfo.Username}&verifytoken={accountInfo.VerifyToken}"
            };
            string uri = builder.Uri.ToString();
#if UNITY_EDITOR
            Debug.Log("Try send for verify: " + uri);
#endif
            byte[] data = await uri.SendGetRequestAsync();
            if (data == null)
            {
                Debug.LogError($"RePhigros API: Unable to connect to server when verifying");
                return (StatusCode.Unknown, null);
            }

            string result = Encoding.UTF8.GetString(data);
            var res = JsonConvert.DeserializeObject<VerifyRequest>(result);
            if (res == null || res.status == false)
            {
                Debug.LogError($"RePhigros API: Error verifying, with code {(int)(res?.Code ?? StatusCode.Unknown)}");
                return (res?.Code ?? StatusCode.Unknown, null);
            }

            if (res.Code != StatusCode.OK) throw new ArgumentException("吃席");
            Debug.Log($"RePhigros API: Access granted");
            return (StatusCode.OK, res.verifyToken);
        }

        private static string ProtectToken(string token)
        {
            return token.Substring(0, 7) + string.Concat(Enumerable.Repeat("*", token.Length - 14)) +
                   token.Substring(token.Length - 7);
        }

        private static readonly Regex Regex = new Regex("[^a-zA-Z0-9]");
        private string username, password;
        
        private void CheckUsername(string input)
        {
            if (Regex.IsMatch(input))
            {
                usernameInputField.text = username;
            }
            else
            {
                username = usernameInputField.text;
            }
        }

        private void CheckPassword(string input)
        {
            if (Regex.IsMatch(input))
            {
                passwordInputField.text = password;
            }
            else
            {
                password = passwordInputField.text;
            }
        }
    }
}