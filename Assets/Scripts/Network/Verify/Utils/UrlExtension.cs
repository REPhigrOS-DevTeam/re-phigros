using System;
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

        public static string SendGetRequest(this string url)
        {
            var uwr = UnityWebRequest.Get(url);
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
    }
}