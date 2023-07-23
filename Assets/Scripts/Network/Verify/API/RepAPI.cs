using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using MainCore.Common;
using Network.Verify.Serialized;
using Network.Verify.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Network.Verify.API
{
    public static class RepAPI
    {
        public static bool RememberMe;

        private static readonly string APIBase = Encoding.ASCII.GetString(new byte[]
        {
            104, 116, 116, 112, 115, 58, 47, 47, 97, 112, 105, 46, 114, 101, 112, 104, 105, 103, 114, 111, 115, 46, 116,
            111, 112
        });

        private static readonly string ManifestDirectory = Encoding.ASCII.GetString(new byte[]
            { 109, 97, 110, 105, 102, 101, 115, 116, 46, 106, 115, 111, 110 });

        private static string loginUrl;

        private static Manifest manifest;
        private static string verifyUrl;

        private static bool inited = false;
        public static string Username = "";
        public static string VerifyToken = "";

        public static void Init(bool refresh = false)
        {
            if (!refresh && inited)
            {
                throw new ArgumentException("Has inited");
            }

            inited = true;
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

            var res = JsonConvert.DeserializeObject<Base>(APIBase.SendGetRequest());
            if (res is not { status: "OK" })
            {
                throw new HttpRequestException($"RePhigros API Service Error, Error code: {res.status}");
            }

            GetManifest();
        }

        private static void GetManifest()
        {
            var res = JsonConvert.DeserializeObject<Manifest>(APIBase.UrlCombine(ManifestDirectory).SendGetRequest());
            manifest = res;
            if (manifest == null)
            {
                throw new HttpRequestException($"RePhigros API: Service Error while getting manifest");
            }

            loginUrl = manifest.apiURL.userlogin;
            verifyUrl = manifest.apiURL.userverify;
            Debug.Log("RePhigros API: Manifest got.");
        }

        public static StatusCode Login(string username, string password)
        {
            var builder = new UriBuilder(APIBase.UrlCombine(loginUrl))
            {
                Query = $"username={username}&password={password}"
            };
            string uri = builder.Uri.ToString();
            // Debug.Log("Try send for login: " + uri);
            var res = JsonConvert.DeserializeObject<VerifyRequest>(uri.SendGetRequest());
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

        public static StatusCode Verify()
        {
            if (Username == "" || VerifyToken == "")
            {
                Debug.LogError("RePhigros API: Undefined behaviour detected, trying to verify without login.");
            }

            var builder = new UriBuilder(APIBase.UrlCombine(verifyUrl))
            {
                Query = $"username={Username}&verifytoken={VerifyToken}"
            };
            string uri = builder.Uri.ToString();
            // Debug.Log("Try send for verify: " + uri);
            var res = JsonConvert.DeserializeObject<VerifyRequest>(uri.SendGetRequest());
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

        public static bool IsLoggedIn()
        {
            return Username != "" && VerifyToken != "";
        }

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
            return token.Substring(0, 7) + string.Concat(Enumerable.Repeat("*", token.Length - 14)) + token.Substring(token.Length - 7);
        }
    }
}