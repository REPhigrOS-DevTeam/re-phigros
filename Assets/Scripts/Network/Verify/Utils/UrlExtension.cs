using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

        [ItemCanBeNull]
        public static async Task<string> SendGetRequestAsync(this string url)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage message = await httpClient.GetAsync(url);
            try
            {
                message.EnsureSuccessStatusCode();
                return await message.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                Debug.LogError($"Error while requesting {url}, http code: {message.StatusCode}");
                return null;
            }
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