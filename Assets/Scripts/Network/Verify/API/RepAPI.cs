using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

        public static async Task<bool> Init(bool refresh = false)
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

            byte[] data = await APIBase.SendGetRequestAsync();
            if (data == null)
            {
                Debug.Log("Error while connect to server root page");
                return false;
            }
            string result = Encoding.UTF8.GetString(data);

            var res = JsonConvert.DeserializeObject<Base>(result);
            if (res is not { status: "OK" })
            {
                throw new HttpRequestException($"RePhigros API Service Error, Error code: {res.status}");
            }

            return await GetManifest();
        }

        private static async Task<bool> GetManifest()
        {
            byte[] data = await APIBase.UrlCombine(ManifestDirectory).SendGetRequestAsync();
            if (data == null)
            {
                Debug.Log("Error while get server manifest");
                return false;
            }
            string result = Encoding.UTF8.GetString(data);
            manifest = JsonConvert.DeserializeObject<Manifest>(result);
            if (manifest == null)
            {
                throw new HttpRequestException($"RePhigros API: Service Error while getting manifest");
            }

            loginUrl = manifest.apiURL.userlogin;
            verifyUrl = manifest.apiURL.userverify;
            Debug.Log("RePhigros API: Manifest got.");
            return true;
        }

        public static async Task<StatusCode> Login(string username, string password)
        {
#if !UNITY_EDITOR
            Debug.Log("Try login");
#endif
            var builder = new UriBuilder(APIBase.UrlCombine(loginUrl))
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
            var res = JsonConvert.DeserializeObject<VerifyRequest>(result) ;
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

            var builder = new UriBuilder(APIBase.UrlCombine(verifyUrl))
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
    }
}