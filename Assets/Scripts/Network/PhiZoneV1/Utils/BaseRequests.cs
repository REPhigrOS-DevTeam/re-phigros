using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Network.PhiZoneV1.Data;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Network.PhiZoneV1.Utils
{
    public static class PhiZoneUrlRequest
    {
        private const string ApiHost = "https://api.phi.zone/";
        private static readonly JObject ConnectErrorData = new() { { "error", "connect_error" } };
        private static readonly Response ConnectErrorResponse = new(-1, ConnectErrorData);

        public delegate void ResponseReceiver(Response response);

        [ItemCanBeNull]
        public static async Task<Response> RequestPhiZone(this string url, string method, bool useToken = true,
            Dictionary<string, object> body = null)
        {
            HttpWebRequest httpWebRequest = WebRequest.CreateHttp(url);
            httpWebRequest.Method = method.ToUpperInvariant();
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add("Accept-Language", ProgramInfo.acceptLanguage);
            httpWebRequest.Headers.Add("User-Agent", ProgramInfo.UserAgent);
            if (useToken)
            {
                httpWebRequest.Headers.Add("Authorization", $"Bearer {UserInfo.token}");
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

        public static async UniTask RequestPhiZone(this string path, string method, ResponseReceiver response,
            bool useToken = true, Dictionary<string, object> body = null)
        {
            UnityWebRequest unityWebRequest = new UnityWebRequest(ApiHost + path);
            unityWebRequest.method = method;
            unityWebRequest.SetRequestHeader("Accept-Language", ProgramInfo.acceptLanguage);
            unityWebRequest.SetRequestHeader("User-Agent", ProgramInfo.UserAgent);
            if (useToken)
            {
                unityWebRequest.SetRequestHeader("Authorization", $"Bearer {UserInfo.token}");
            }

            if (body != null)
            {
                unityWebRequest.uploadHandler =
                    new UploadHandlerRaw(Encoding.UTF8.GetBytes(JObject.FromObject(body).ToString()));
            }

            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            await unityWebRequest.SendWebRequest();

            response?.Invoke(unityWebRequest.result == UnityWebRequest.Result.ConnectionError
                ? ConnectErrorResponse
                : new Response(unityWebRequest.responseCode, JObject.Parse(unityWebRequest.downloadHandler.text)));
        }
    }
}