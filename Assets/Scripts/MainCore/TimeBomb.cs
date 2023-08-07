using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace MainCore
{
    public static class TimeBomb
    {
        public static async Task<bool> IsInRange(DateTime end, bool useUtc)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage message = await httpClient.GetAsync("http://quan.suning.com/getSysTime.do");
            try
            {
                message.EnsureSuccessStatusCode();
                JObject jObject = JObject.Parse(await message.Content.ReadAsStringAsync());
                string dateStr = jObject["sysTime1"].ToString();
                DateTime dateTime1 = new DateTime(int.Parse(dateStr.Substring(0, 4)), int.Parse(dateStr.Substring(4, 2)),
                    int.Parse(dateStr.Substring(6, 2)), int.Parse(dateStr.Substring(8, 2)),
                    int.Parse(dateStr.Substring(10, 2)), int.Parse(dateStr.Substring(12, 2)));
                if (useUtc) dateTime1 = dateTime1.ToUniversalTime();
                return dateTime1 <= end;
            }
            catch (HttpRequestException)
            {
                DateTime dateTime = DateTime.Now;
                if (useUtc) dateTime = dateTime.ToUniversalTime();
                return (dateTime <= end);
            }
        }
    }
}