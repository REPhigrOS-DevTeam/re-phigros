using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Network.PhiZone.Data;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Network.PhiZone.Utils
{
    public static class PhiZoneUrlRequest
    {
        private const string ApiHost = "https://api.phizone.cn/";
        private static readonly JObject ConnectErrorData = new() { { "error", "connect_error" } };
        private static readonly Response ConnectErrorResponse = new(-1, ConnectErrorData);

        [ItemCanBeNull]
        public static async Task<Response> RequestPhiZoneWithHttpClient(this string url, string method, bool useToken = true,
            Dictionary<string, string> body = null)
        {
            HttpWebRequest httpWebRequest = WebRequest.CreateHttp(url);
            httpWebRequest.Method = method.ToUpperInvariant();
            httpWebRequest.ContentType = "application/json";
            // httpWebRequest.Headers.Add("Accept-Language", ProgramInfo.acceptLanguage);
            // httpWebRequest.Headers.Add("User-Agent", ProgramInfo.UserAgent);
            if (useToken)
            {
                httpWebRequest.Headers.Add("Authorization", $"Bearer {PhiZoneGlobalVarieties.User.AccessToken}");
            }
            if (body != null)
            {
                using StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                await streamWriter.WriteAsync(JObject.FromObject(body).ToString());
            }

            using HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            using var streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string data = await streamReader.ReadToEndAsync();

            return new Response((int) httpWebResponse.StatusCode, JObject.Parse(data));
        }

        public static async UniTask<Response> RequestPhiZoneWithUwr(this string path, string method, bool useToken = true, Dictionary<string, string> body = null)
        {
            UnityWebRequest unityWebRequest = new UnityWebRequest(ApiHost.UrlCombine(path), method);
            // unityWebRequest.SetRequestHeader("Accept-Language", ProgramInfo.acceptLanguage);
            // unityWebRequest.SetRequestHeader("User-Agent", ProgramInfo.UserAgent);
            unityWebRequest.SetRequestHeader("Content-Type", "x-www-form-urlencoded");
            if (useToken)
            {
                unityWebRequest.SetRequestHeader("Authorization", $"Bearer {PhiZoneGlobalVarieties.User.AccessToken}");
            }

            if (body != null)
            {
                unityWebRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(string.Join("&",
                    body.Select(pair =>
                        UnityWebRequest.EscapeURL(pair.Key, Encoding.UTF8) + "=" +
                        UnityWebRequest.EscapeURL(pair.Value, Encoding.UTF8)))));
                // unityWebRequest.uploadHandler =
                //     new UploadHandlerRaw(Encoding.UTF8.GetBytes(JObject.FromObject(body).ToString()));
            }

            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            await unityWebRequest.SendWebRequest();

            string text = unityWebRequest.downloadHandler.text;
            return unityWebRequest.result == UnityWebRequest.Result.ConnectionError
                ? ConnectErrorResponse
                : new Response(unityWebRequest.responseCode, text.StartsWith("{") ? JObject.Parse(text) : ConnectErrorData);
        }
    }
}