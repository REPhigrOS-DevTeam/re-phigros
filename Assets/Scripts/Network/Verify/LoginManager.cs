using System;
using System.Text.RegularExpressions;
using MainCore.Common;
using MainCore.Utilities;
using Network.Verify;
using Network.Verify.API;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField ifUsername, ifPassword;
    public Toggle tRememberMe;
    public Button bLogin;
    private static readonly Regex Regex = new Regex("[^a-zA-Z0-9]");
    private string username = "", password = "";
    private bool firstLogin = true;

    private void Awake()
    {
        ifUsername.onEndEdit.AddListener(CheckUsername);
        ifPassword.onEndEdit.AddListener(CheckPassword);
        bLogin.onClick.AddListener(Login);
        tRememberMe.isOn = RepAPI.RememberMe;
        if (RepAPI.RememberMe)
        {
            ifUsername.text = username = RepAPI.Username;
            ifPassword.text = "0000000000000000";
        }
        tRememberMe.onValueChanged.AddListener(_ => OnInputFieldsClicked(null));
        AddInputFieldClickEvent(ifUsername, OnInputFieldsClicked);
        AddInputFieldClickEvent(ifPassword, OnInputFieldsClicked);
    }

    private void OnInputFieldsClicked(BaseEventData data)
    {
        if (!RepAPI.RememberMe) return;
        RepAPI.RememberMe = false;
        firstLogin = false;
        ifPassword.text = password = "";
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

    private void AddInputFieldClickEvent(InputField inputField, UnityAction<BaseEventData> selectEvent) //可以在Awake中调用
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

        onClick.callback.AddListener(selectEvent);
        eventTrigger.triggers.Add(onClick);
    }

    private void Login()
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

        bool lastRememberMe = RepAPI.RememberMe;
        RepAPI.RememberMe = tRememberMe.isOn;
        RepAPI.SaveRememberMe();
        if (lastRememberMe && RepAPI.RememberMe)
        {
            StatusCode code = RepAPI.Verify();
            switch (code)
            {
                case StatusCode.Unknown:
                    InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了未定义的状态码，请联系开发者\n程序即将退出", Util.QuitApp, "确定");
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
            StatusCode code = RepAPI.Login(username, password);
            switch (code)
            {
                case StatusCode.Unknown:
                    InGameUIManager.ShowModalWindowWithClose("致命错误", "收到了未定义的状态码，请联系开发者\n程序即将退出", Util.QuitApp, "确定");
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
                    InGameUIManager.ShowModalWindowWithClose("错误", "用户名不合法", () => { }, "确定");
                    break;
                case StatusCode.InvalidPassword:
                    InGameUIManager.ShowModalWindowWithClose("错误", "密码不合法", () => { }, "确定");
                    break;
                case StatusCode.NoPermission:
                    InGameUIManager.ShowModalWindowWithClose("错误", "没有内侧权限", () => { }, "确定");
                    break;
                case StatusCode.InvalidToken:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void OnLoginSucceeded()
    {
        if (RepAPI.RememberMe) RepAPI.SaveUsernameAndToken();
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
}