using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace MainCore
{
    public class TimeBomb
    {
        public delegate void ResultReceiver(bool result);

        public static IEnumerator IsInRange(DateTime end, bool useUtc, ResultReceiver receiver)
        {
            UnityWebRequest uwr = UnityWebRequest.Get("http://quan.suning.com/getSysTime.do");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                DateTime dateTime = DateTime.Now;
                if (useUtc) dateTime = dateTime.ToUniversalTime();
                receiver.Invoke(dateTime <= end);
                yield break;
            }

            JObject jObject = JObject.Parse(uwr.downloadHandler.text);
            string dateStr = jObject["sysTime1"].ToString();
            DateTime dateTime1 = new DateTime(int.Parse(dateStr.Substring(0, 4)), int.Parse(dateStr.Substring(4, 2)),
                int.Parse(dateStr.Substring(6, 2)), int.Parse(dateStr.Substring(8, 2)),
                int.Parse(dateStr.Substring(10, 2)), int.Parse(dateStr.Substring(12, 2)));
            if (useUtc) dateTime1 = dateTime1.ToUniversalTime();
            receiver.Invoke(dateTime1 <= end);
        }
    }
}