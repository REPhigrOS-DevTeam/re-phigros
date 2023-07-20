using System;
using System.Net.Http;
using JetBrains.Annotations;
using Network.Verify.Serialized;
using Network.Verify.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Network.Verify.API
{
    public class RepAPI
    {
        private const string APIBase = "https://api.rephigros.top";
        private const string ManifestDirectory = "manifest.json";

        private string loginUrl;

        private Manifest manifest;
        private string verifyUrl;

        public static string Username
        {
            get => PlayerPrefs.GetString("repapi_playername", "");
            set
            {
                PlayerPrefs.SetString("repapi_playername", value);
                PlayerPrefs.Save();
            }
        }
        
        public static string VerifyToken
        {
            get => PlayerPrefs.GetString("repapi_verifytoken", "");
            set
            {
                PlayerPrefs.SetString("repapi_verifytoken", value);
                PlayerPrefs.Save();
            }
        }

        public RepAPI()
        {
            var res = JsonConvert.DeserializeObject<Base>(APIBase.SendGetRequest());
            if (res is not {status: "OK"})
            {
                throw new HttpRequestException($"RePhigros API Service Error, Error code: {res.status}");
            }

            GetManifest();
        }

        private void GetManifest()
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

        public StatusCode Login(string username, string password)
        {
            var builder = new UriBuilder(APIBase.UrlCombine(loginUrl))
            {
                Query = $"username={username}&password={password}"
            };
            var res = JsonConvert.DeserializeObject<VerifyRequest>(builder.Uri.ToString().SendGetRequest());
            if (res == null || res.status == false)
            {
                Debug.LogError($"RePhigros API: Error logging in, with code {(int) (res?.Code ?? StatusCode.Unknown)}");
                return res?.Code ?? StatusCode.Unknown;
            }

            Debug.Log($"RePhigros API: Successfully logged in with verifyToken: {res.verifyToken}");
            SaveUsernameAndToken(username, res.verifyToken);
            return StatusCode.OK;
        }

        public StatusCode Verify()
        {
            string userName = Username;
            string verifyToken = Username;
            if (userName == "" || verifyToken == "")
            {
                Debug.LogError("RePhigros API: Undefined behaviour detected, trying to verify without login.");
            }

            var builder = new UriBuilder(APIBase.UrlCombine(verifyUrl))
            {
                Query = $"username={userName}&verifytoken={verifyToken}"
            };
            var res = JsonConvert.DeserializeObject<VerifyRequest>(builder.Uri.ToString().SendGetRequest());
            if (res == null || res.status == false)
            {
                Debug.LogError($"RePhigros API: Error verifying, with code {(int) (res?.Code ?? StatusCode.Unknown)}");
                return res?.Code ?? StatusCode.Unknown;
            }

            Debug.Log($"RePhigros API: Access granted");
            SaveUsernameAndToken(userName, res.verifyToken);
            return StatusCode.OK;
        }

        public bool IsLoggedIn()
        {
            return Username != "" && VerifyToken != "";
        }

        private void SaveUsernameAndToken([CanBeNull] string username = null, [CanBeNull] string token = null)
        {
            if (username != null) PlayerPrefs.SetString("repapi_playername", username);
            if (token != null) PlayerPrefs.SetString("repapi_verifytoken", token);
            PlayerPrefs.Save();
        }

        private void ResetUsernameAndToken()
        {
            PlayerPrefs.SetString("repapi_playername", "");
            PlayerPrefs.SetString("repapi_verifytoken", "");
            PlayerPrefs.Save();
        }
    }
}