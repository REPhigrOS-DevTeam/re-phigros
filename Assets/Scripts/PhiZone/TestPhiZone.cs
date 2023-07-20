using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MainCore.UI;
using PhiZone.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace PhiZone
{
    public class TestPhiZone : MonoBehaviour
    {
        public InputField ifEmail, ifPassword;
        public Button bLogin;
        private string email, password;
        private static readonly Regex EmailRegex = new("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");

        public void Start()
        {
            ifEmail.onEndEdit.AddListener(CheckEmail);
            ifPassword.onEndEdit.AddListener(CheckPassword);
            bLogin.onClick.AddListener(Login);
        }

        private void CheckEmail(string value)
        {
            if (EmailRegex.IsMatch(value))
            {
                email = value;
            }
            else
            {
                PopupMessageManager.Instance.ChangeContent("错误：邮箱不合法");
                ifEmail.text = email;
            }
        }

        private void CheckPassword(string value)
        {
            password = value;
        }

        private void Login()
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                PopupMessageManager.Instance.ChangeContent("错误：密码或邮箱为空");
                return;
            }

            var body = new Dictionary<string, object>();
            body.Add("client_id", ProgramInfo.ClientId);
            body.Add("client_secret", ProgramInfo.ClientSecret);
            body.Add("grant_type", "password");
            body.Add("username", email);
            body.Add("password", password);

            StartCoroutine("/auth/token".RequestPhiZone("POST", LoginResponse, false));
        }

        private void LoginResponse(Response response)
        {
        }
    }
}