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

        private async void Login()
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                PopupMessageManager.Instance.ChangeContent("错误：密码或邮箱为空");
                return;
            }

            var body = new Dictionary<string, object>
            {
                { "client_id", ProgramInfo.ClientId },
                { "client_secret", ProgramInfo.ClientSecret },
                { "grant_type", "password" },
                { "username", email },
                { "password", password }
            };

            StartCoroutine("/auth/token".RequestPhiZone("POST", LoginResponse, false, body));
        }

        private void LoginResponse(Response response)
        {
            // {
            //     "access_token": "6pHmoGzwWRJZ9NH7zN1a5rYBhgbKz9",
            //     "expires_in": 43200,
            //     "token_type": "Bearer",
            //     "scope": "read write",
            //     "refresh_token": "YsbxZZ8s1lRGWKl2mPrtWh4YUCNZgw"
            // }
        }
    }
}