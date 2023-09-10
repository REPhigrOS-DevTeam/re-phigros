using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Network.Account.Utils;
using Network.API;
using Network.Multiplayer.Managers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Network.Chart
{
    public class ChartHandler
    {
        private static Dictionary<string, string> chartMap = new Dictionary<string, string>();

        private static List<string> downloadedCharts = new List<string>();

        public static async Task<string> Upload(string filePath)
        {
            filePath = Path.GetFullPath(filePath);
            if (chartMap.ContainsKey(filePath)) return chartMap[filePath];
            using MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(await File.ReadAllBytesAsync(filePath)), "file", Path.GetFileName(filePath));
            content.Add(new StringContent(SocketManager.GetServerId()), "serverid");
            content.Add(new StringContent(SocketManager.GetRoomId()), "roomid");
            JObject response = JObject.Parse(await RepAPI.ChartUrlBase.UrlCombine("upload").PostWithHttpClient(content));
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
            JObject response = JObject.Parse(await RepAPI.ChartUrlBase.UrlCombine("upload").PostWithHttpClient(content));
            if (response.Properties().ToArray().Length == 0)
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
            if (downloadedCharts.Contains(id)) return await File.ReadAllBytesAsync($"{Application.temporaryCachePath}/online_charts/{id}");
            byte[] bytes = await (RepAPI.ChartUrlBase.UrlCombine("download") + $"?scoreid={id}").SendGetRequestAsync();
            await File.WriteAllBytesAsync($"{Application.temporaryCachePath}/online_charts/{id}", bytes);
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
            chartMap.Clear();
            downloadedCharts.Clear();
        }

        public static void OnRoomQuited()
        {
            chartMap.Clear();
            downloadedCharts.Clear();
            string[] strings = Directory.GetFiles($"{Application.temporaryCachePath}/online_charts");
            foreach (string s in strings)
            {
                File.Delete(s);
            }
        }
    }
}