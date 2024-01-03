using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MainCore.Utilities;
using Network.Account.Serialized;
using Network.Account.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Network
{
    public static class RepAPI
    {
        private static readonly string HostOriginal = Encoding.ASCII.GetString(new byte[]
        {
            97, 112, 105, 46, 114, 101, 112, 104, 105, 103, 114, 111, 115, 46, 116,
            111, 112
        }); // api.rephigros.top

        private static readonly string HostMirror = Encoding.ASCII.GetString(new byte[]
        {
            97, 112, 105, 46, 114, 101, 112, 97, 99, 99, 46, 109, 105, 114, 114,
            111, 114, 46, 97, 116, 111, 109, 117, 110, 105, 116, 101, 46, 99, 110
        }); // api.repacc.mirror.atomunite.cn

        private static bool useMirror = false;
        private static bool switched = false;

        public static readonly string ManifestDirectory = Encoding.ASCII.GetString(new byte[]
            { 109, 97, 110, 105, 102, 101, 115, 116, 46, 106, 115, 111, 110 }); // manifest.json

        private static bool inited = false;
        
        public static string GetAPIBase()
        {
            return $"https://{(useMirror ? HostMirror : HostOriginal)}";
        }

        #region AboutManifest

        private static Manifest manifest;
        public static string loginUrl { get; private set; }
        public static string verifyUrl { get; private set; }

        #endregion

        public static async Task<bool> Init()
        {
            if (inited)
            {
                throw new ArgumentException("Has inited");
            }

            inited = true;
            useMirror = PlayerPrefs.GetInt("useMirror", 0) == 1;

            byte[] data;
            try
            {
                data = await GetAPIBase().SendGetRequestAsync(3000);
            }
            catch (Exception e) when (e is WebException or TaskCanceledException)
            {
                if (switched)
                {
                    Debug.Log("访问两个站点都超时");
                    return false;
                }
                switched = true;
                PlayerPrefs.SetInt("useMirror", useMirror ? 0 : 1);
                Debug.Log(!useMirror ? "访问主站超时，切换到镜像站点" : "访问镜像站超时，切换到主站点");
                inited = false;
                return await Init();
            }
            if (data == null)
            {
                Debug.LogError("Error occured while connect to server root page");
                return false;
            }

            string result = Encoding.UTF8.GetString(data);

            var res = JsonConvert.DeserializeObject<Base>(result);
            if (res is not { status: "OK" })
            {
                InGameUIManager.ShowModalWindowWithClose("致命错误", "Re:Phigros服务器内部故障，请联系开发组", Util.QuitApp, "退出程序");
                throw new HttpRequestException($"RePhigros API Service Error, Error code: {res.status}");
            }
            
            Debug.Log($"已连接{(useMirror ? "镜像站" : "主站")}");

            return await GetManifest();
        }

        private static async Task<bool> GetManifest()
        {
            byte[] data = await GetAPIBase().UrlCombine(ManifestDirectory).SendGetRequestAsync();
            if (data == null)
            {
                Debug.LogError("Error while get server manifest");
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
    }
}