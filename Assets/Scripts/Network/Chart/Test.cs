#if UNITY_EDITOR
using System.IO;
using System.Net.Http;
using Network.Account.Utils;
using Network.API;
using UnityEngine;

namespace Network.Chart
{
    public static class Test
    {
        public const string ApiBase = "http://43.248.185.65:45944/api/";
        public static async void Qwq()
        {
            await ChartHandler.Upload("E:/qqData/3120393927/FileRecv/暴力扳机 IN Lv.15.zip");
            string filePath = "E:/qqData/3120393927/FileRecv/暴力扳机 IN Lv.15.zip";
            filePath = Path.GetFullPath(filePath);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(await File.ReadAllBytesAsync(filePath)), "file", Path.GetFileName(filePath));
            content.Add(new StringContent("63ae61e1272f"), "serverid");
            content.Add(new StringContent("114514"), "roomid");
            string postWithHttpClient = await "http://43.248.185.65:45944/api/upload".PostWithHttpClient(content);
            Debug.Log(postWithHttpClient);
        }
    }
}
#endif