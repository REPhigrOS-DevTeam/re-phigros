using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network.Account.Utils
{
    public class AccountManager
    {
        private const string Key = "repapi_accountlist";

        public static List<AccountInfo> GetAccountList()
        {
            List<AccountInfo> accountInfos = new List<AccountInfo>();
            string[] lines = PlayerPrefs.GetString(Key).Split("\n");
            for (int i = 0; i < lines.Length - 1; i+=2)
            {
                accountInfos.Add(new AccountInfo(lines[i], lines[i+1]));
            }
            accountInfos.Reverse();
            return accountInfos;
        }

        public static void SaveAccountList(List<AccountInfo> accountInfos)
        {
            accountInfos.Reverse();
            PlayerPrefs.SetString(Key, string.Join("\n", accountInfos.Select(info => $"{info.Username}\n{info.VerifyToken}")));
            PlayerPrefs.Save();
        }
        
        public class AccountInfo
        {
            public string Username;
            public string VerifyToken;

            public AccountInfo(string username = "", string verifyToken = "")
            {
                Username = username;
                VerifyToken = verifyToken;
            }
        }
    }
}