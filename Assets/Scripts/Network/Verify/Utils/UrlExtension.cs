using System;
using System.Collections;
using System.Threading.Tasks;
using MainCore.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace Network.Verify.Utils
{
    public static class UrlExtension
    {
        public static string UrlCombine(this string urlBase, string combined)
        {
            return new Uri(new Uri(urlBase), combined).ToString();
        }
        
        public static string SendGetRequestSync(this string url)
        {
            var uwr = UnityWebRequest.Get(url);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error while requesting {url}, code: {uwr.responseCode}, message: {uwr.error}");
                return "ERROR";
            }

            return uwr.downloadHandler.text;
        }

        public static async Task<string> SendGetRequestAsync(this string url)
        {
            var uwr = UnityWebRequest.Get(url);
            await uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error while requesting {url}, code: {uwr.responseCode}, message: {uwr.error}");
                return "ERROR";
            }

            return uwr.downloadHandler.text;
        }

        public static IEnumerator SendGetRequest(this string url, Action<string?> callback)
        {
            var uwr = UnityWebRequest.Get(url);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error while requesting {url}, code: {uwr.responseCode}, message: {uwr.error}");
                callback.Invoke(null);
            } else callback.Invoke(uwr.downloadHandler.text);
        }
    }
}