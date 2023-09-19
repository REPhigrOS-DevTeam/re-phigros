using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MainCore.Utilities;
using Network.Account.Utils;
using Network.Multiplayer.Managers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Network.Chart
{
    public class ChartHandler
    {
        private static Dictionary<string, string> chartMap = new Dictionary<string, string>(); // 给房主用的, key是path，value是id

        private static List<string> downloadedCharts = new List<string>(); // 给房员用的，下载好的文件我存在程序的tmp文件夹乐

        private static string UserSpace = "";

        public static string TmpPathRoot
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (UserSpace == "") UserSpace = Path.GetFullPath(Application.persistentDataPath + "/../../../..");
                string path = UserSpace + "/RPGRCache";
                Debug.Log(path);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
#else
                return Application.temporaryCachePath;
#endif
            }
        }

        public static async Task<string> Upload(string folderPath)
        {
            if (!Directory.Exists(TmpPathRoot + "/zip_charts")) Directory.CreateDirectory(TmpPathRoot + "/zip_charts");
            string filePath = Path.GetFullPath(TmpPathRoot + "/zip_charts/" + Path.GetFileName(folderPath) + ".zip");
            if (chartMap.ContainsKey(filePath)) return chartMap[filePath];
            ZipUtils.ZipDirectory(folderPath, filePath);
            byte[] zipResult = await File.ReadAllBytesAsync(filePath);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(zipResult), "file", Path.GetFileName(filePath));
            content.Add(new StringContent(SocketManager.GetServerId()), "serverid");
            content.Add(new StringContent(SocketManager.GetRoomId()), "roomid");
            JObject response = JObject.Parse(await RepAPI.ChartUrlBase.UrlCombine("/upload").PostWithHttpClient(content));
            if (response["status"].ToObject<bool>())
            {
                string id = response["scoreid"].ToString();
                chartMap.Add(filePath, id);
                return id;
            }

            Debug.LogError($"上传谱面{filePath}失败");
            throw new ArgumentException();
        }
        
        private static async void Delete(string id)
        {
            using MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent(id), "scoreid");
            Debug.Log("Chart to delete: " + id);
            var postWithHttpClient = await RepAPI.ChartUrlBase.UrlCombine("/delete").PostWithHttpClient(content);
            Debug.Log("Response: \n" + postWithHttpClient);
            // TODO: SKy这个沙雕写了个bug
            JObject response = JObject.Parse(postWithHttpClient.Substring(postWithHttpClient.LastIndexOf('{')));
            if (response["status"].ToObject<bool>())
            {
                chartMap.Remove(chartMap.Where(kvp => kvp.Value == id).ToArray()[0].Key);
            }
            else
            {
                Debug.LogError($"删除谱面{id}失败");
                throw new ArgumentException();
            }
        }

        public static async Task<byte[]> Download(string id)
        {
            if (downloadedCharts.Contains(id)) return await File.ReadAllBytesAsync($"{TmpPathRoot}/online_charts/{id}");
            byte[] bytes = await (RepAPI.ChartUrlBase.UrlCombine("/download") + $"?scoreid={id}").SendGetRequestAsync();
            if (!Directory.Exists($"{TmpPathRoot}/online_charts/{id}")) Directory.CreateDirectory($"{TmpPathRoot}/online_charts/{id}");
            await File.WriteAllBytesAsync($"{TmpPathRoot}/online_charts/{id}", bytes);
            downloadedCharts.Add(id);
            return bytes;
        }

        public static async Task<byte[]> DownloadFromPhiZone(string id)
        {
            return Array.Empty<byte>();
        }

        public static void OnRoomClosed()
        {
            foreach (string id in chartMap.Values)
            {
                Delete(id);
            }
            downloadedCharts.Clear();
        }

        public static void OnRoomQuited()
        {
            chartMap.Clear();
            downloadedCharts.Clear();
            if (!Directory.Exists($"{TmpPathRoot}/online_charts")) return;
            string[] strings = Directory.GetFiles($"{TmpPathRoot}/online_charts");
            foreach (string s in strings)
            {
                File.Delete(s);
            }
        }
    }
}