using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Network
{
    public static class UrlExtension
    {
        public static string UrlCombine(this string urlBase, string combined)
        {
            return $"{urlBase}{(urlBase.EndsWith("/") || combined.StartsWith("/") ? "" : "/")}{combined}";
        }

        [ItemCanBeNull]
        public static async Task<byte[]> SendGetRequestAsync(this string url, int timeOut = -1, bool disableCache = true)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                (requestMessage, certificate, chain, errors) =>
                {
                    if (errors == SslPolicyErrors.None) return true;
                    if (certificate == null || requestMessage.RequestUri == null) return false;
                    return certificate.Issuer == "CN=R3, O=Let's Encrypt, C=US" &&
                           certificate.Subject == "CN=rephigros.top" &&
                           (requestMessage.RequestUri.Host == "rephigros.top" ||
                            requestMessage.RequestUri.Host.EndsWith(".rephigros.top"));
                };
            HttpClient httpClient = new HttpClient(httpClientHandler);
            if (disableCache) httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true
            };
            httpClient.Timeout = timeOut < 0 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeOut);
            HttpResponseMessage message = null;
            try
            {
                message = await httpClient.GetAsync(url);
                message.EnsureSuccessStatusCode();
                return await message.Content.ReadAsByteArrayAsync();
            }
            catch (Exception e) when (e is WebException or TaskCanceledException)
            {
                Debug.LogError($"Timeout occurred while requesting {url}");
                throw;
            }
            catch (HttpRequestException)
            {
                if (message != null) Debug.LogError($"Error while requesting {url}, http code: {message.StatusCode}");
                else throw new WebException();
                return null;
            }
            finally
            {
                httpClientHandler.Dispose();
                httpClient.Dispose();
            }
        }

        private static void WriteServerCertificationInfo(HttpRequestMessage requestMessage,
            X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
        {
            StreamWriter streamWriter =
                new StreamWriter(Application.dataPath + "/test.txt", false, new UTF8Encoding(false));
            // It is possible to inspect the certificate provided by the server.
            streamWriter.WriteLine($"Requested URI: {requestMessage.RequestUri}");
            streamWriter.WriteLine($"Effective date: {certificate?.GetEffectiveDateString()}");
            streamWriter.WriteLine($"Exp date: {certificate?.GetExpirationDateString()}");
            streamWriter.WriteLine($"Issuer: {certificate?.Issuer}");
            streamWriter.WriteLine($"Subject: {certificate?.Subject}");

            // Based on the custom logic it is possible to decide whether the client considers certificate valid or not
            streamWriter.WriteLine($"Errors: {errors}");
        }

        public static async Task<string> PostWithHttpClient(this string url, MultipartFormDataContent content = null)
        {
            Debug.Log("URL: " + url);
            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync(url, content ?? new MultipartContent());
            return await response.Content.ReadAsStringAsync();
        }
    }
}