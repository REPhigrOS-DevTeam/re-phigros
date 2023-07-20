using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace PhiZone.Utils
{
    public static class PhiZoneUrlRequest
    {
        private const string ApiHost = "https://api.phi.zone/";
        private static readonly JObject ConnectErrorData = new() { { "error", "connect_error" } };
        private static readonly Response ConnectErrorResponse = new(-1, ConnectErrorData);

        public delegate void ResponseReceiver(Response response);

        public static IEnumerator RequestPhiZone(this string path, string method, ResponseReceiver response,
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

            yield return unityWebRequest.SendWebRequest();

            response?.Invoke(unityWebRequest.result == UnityWebRequest.Result.ConnectionError
                ? ConnectErrorResponse
                : new Response(unityWebRequest.responseCode, JObject.Parse(unityWebRequest.downloadHandler.text)));
        }
    }
}