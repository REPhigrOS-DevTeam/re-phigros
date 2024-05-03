using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network.Account.Utils
{
    public class AccountManager
    {
        private const string ListKey = "repapi_accountlist";
        private const string LastUserKey = "repapi_lastuser";

        public static List<AccountInfo> GetAccountList()
        {
            List<AccountInfo> accountInfos = new List<AccountInfo>();
            string[] lines = PlayerPrefs.GetString(ListKey).Split("\n");
            for (int i = 0; i < lines.Length - 1; i+=2)
            {
                accountInfos.Add(new AccountInfo(lines[i], lines[i+1]));
            }
            accountInfos.Reverse();
            return accountInfos;
        }

        public static void SaveAccountList(List<AccountInfo> accountInfos, string lastUser = "")
        {
            List<AccountInfo> infos = accountInfos.ConvertAll(accountInfo => new AccountInfo(accountInfo.Username, accountInfo.VerifyToken));
            infos.Reverse();
            PlayerPrefs.SetString(ListKey, string.Join("\n", infos.Select(info => $"{info.Username}\n{info.VerifyToken}")));
            PlayerPrefs.Save();
            if (lastUser != "") PlayerPrefs.SetString(LastUserKey, lastUser);
        }

        public static string GetLastUser() => PlayerPrefs.GetString(LastUserKey, "");
        
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