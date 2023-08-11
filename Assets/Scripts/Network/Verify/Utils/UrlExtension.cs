using System;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Network.Verify.Utils
{
    public static class UrlExtension
    {
        public static string UrlCombine(this string urlBase, string combined)
        {
            return new Uri(new Uri(urlBase), combined).ToString();
        }

        [ItemCanBeNull]
        public static async Task<byte[]> SendGetRequestAsync(this string url)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage message = await httpClient.GetAsync(url);
            try
            {
                message.EnsureSuccessStatusCode();
                return await message.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException)
            {
                Debug.LogError($"Error while requesting {url}, http code: {message.StatusCode}");
                return null;
            }
        }

        public static async Task<string> PostWithHttpClient(this string url, MultipartFormDataContent content = null)
        {
            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync(url, content ?? new MultipartContent());
            return await response.Content.ReadAsStringAsync();
        }
    }
}