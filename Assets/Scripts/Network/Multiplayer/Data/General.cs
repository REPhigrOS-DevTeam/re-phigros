using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MainCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Network.Multiplayer.Data
{
    public static class General
    {
        private static readonly UTF8Encoding NoBomUtf8Encoding = new(false);

        public static int Send(this Socket socket, object? value)
        {
            string messageToSend = JsonConvert.SerializeObject(value, Formatting.None);
            Debug.Log("尝试发送" + messageToSend);
            return socket.Send(NoBomUtf8Encoding.GetBytes(messageToSend));
        }

        public static JObject[]? Receive(this Socket socket)
        {
            if (socket.Available <= 0) return null;
            byte[] buffer = new byte[8];
            List<byte> dataList = new List<byte>();
            int length;
            qwq:
            while ((length = socket.Receive(buffer)) > 0)
            {
                dataList.AddRange(buffer.Take(length));

                if (socket.Available <= 0 || length < buffer.Length) break;
            }

            if (dataList.Count == 0) throw new NullReferenceException("Received Nothing");
            if (socket.Available > 0) goto qwq;

            string s = NoBomUtf8Encoding.GetString(dataList.ToArray());
            Debug.Log("接收到：" + s);
            return SplitSocketPacks(s);
        }

        private static JObject[] SplitSocketPacks(string s)
        {
            List<JObject> packs = new List<JObject>();
            bool zhuanYi = false;
            bool isInString = false;
            int j = 0;
            TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(s);
            StringBuilder stringBuilder = new StringBuilder();
            while (textElementEnumerator.MoveNext())
            {
                string element = textElementEnumerator.GetTextElement();
                if (stringBuilder.Length == 0 && element != "{") throw new ArgumentException("你这包有问题啊");
                if (element == "\\")
                {
                    if (!isInString) throw new ArgumentException();
                    zhuanYi = !zhuanYi;
                }
                else
                {
                    if (element == "\"")
                    {
                        if (!zhuanYi)
                            isInString = !isInString;
                    }
                    else if (!isInString)
                    {
                        if (element == "{") j++;
                        if (element == "}") j--;
                    }

                    zhuanYi = false;
                }

                stringBuilder.Append(element);
                if (j != 0) continue;
                string s1 = stringBuilder.ToString();
                packs.Add(JObject.Parse(s1));
                stringBuilder.Clear();
            }

            return packs.ToArray();
        }

        public static bool TryParseHost(string url, out IPEndPoint endPoint, out string displayUrl)
        {
            int maoHaoWeiZhi = url.LastIndexOf(":", StringComparison.Ordinal);
            if (maoHaoWeiZhi < 0)
            {
                endPoint = null;
                displayUrl = "";
                return false;
            }

            string host = url.Substring(0, maoHaoWeiZhi);
            string portStr = url.Substring(maoHaoWeiZhi + 1);
            if (!int.TryParse(portStr, out int port) || port < 0 || port > 65535)
            {
                endPoint = null;
                displayUrl = "";
                return false;
            }

            if (host.StartsWith("[") && host.EndsWith("]"))
            {
                host = host.Substring(1);
                host = host.Substring(0, host.Length - 1);
                if (!IPAddress.TryParse(host, out IPAddress ipAddress) ||
                    ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    endPoint = null;
                    displayUrl = "";
                    return false;
                }

                endPoint = new IPEndPoint(ipAddress, port);
                displayUrl = host + ":" + port;
                return true;
            }

            List<IPAddress> hostAddresses;
            try
            {
                hostAddresses = Dns.GetHostAddresses(host).ToList();
            }
            catch (Exception e) when (e is SocketException or ArgumentException)
            {
                endPoint = null;
                displayUrl = "";
                return false;
            }

            hostAddresses.Sort((a, b) =>
            {
                int c = (int)a.AddressFamily;
                int d = (int)b.AddressFamily;
                return c - d;
            });
            endPoint = new IPEndPoint(hostAddresses[0], port);
            displayUrl = host + ":" + port;
            return true;
        }
    }

    public class GeneralSendData
    {
        [JsonProperty("operate")] public string Operate;
        [JsonProperty("username")] public string Username;
        [JsonProperty("addition")] public Dictionary<string, object> Addition = new();
    }

    public class LoginSendData : GeneralSendData
    {
        [JsonProperty("verifyToken")] public string VerifyToken;
    }

    public class SendDataWithToken : GeneralSendData
    {
        [JsonProperty("loginToken")] public string LoginToken;
    }

    public class GeneralReceiveData
    {
        [JsonProperty("Type")] public string Type;
    }

    public class PingReceiveData : GeneralReceiveData
    {
        [JsonProperty("Status")] public bool Status;
        [JsonProperty("Name")] public string Name;
        [JsonProperty("Motd")] public string Motd;
        [JsonProperty("ReceiveTime")] public long ReceiveTime;
        [JsonProperty("OnlineMode")] public bool IsOnline;
        [JsonProperty("ChartUploadMode")] public bool EnableChartUpload;
    }

    public class BackReceiveData : GeneralReceiveData
    {
        [JsonProperty("Status")] public bool Status;
        [JsonProperty("msg")] public string Message;
    }

    public class ActiveReceiveData : GeneralReceiveData
    {
        [JsonProperty("operate")] public string Operate;
    }

    public class SongInfo
    {
        [JsonProperty("FolderName")] public string FolderName;
        [JsonProperty("SongName")] public string SongName;
        [JsonProperty("SongComposer")] public string SongComposer;
        [JsonProperty("SongDifficulty")] public string SongDifficulty;
        [JsonProperty("SongCharter")] public string SongCharter;
        [JsonProperty("SongIllustrator")] public string SongIllustrator;
        [JsonProperty("MusicLength")] public float MusicLength;
    }

    public enum ClientOperate
    {
        Server_Sync = 0,
        User_LoginToServer,
        User_LeaveServer,
        User_CreateNewRoom,
        User_CloseRoom,
        User_JoinRoom,
        User_QuitRoom,
        User_Ready,
        User_UnReady,
        User_GameEnd,
        Room_UserQuitGame,
        Room_UpdateSong,
        Room_GameStart,
        Room_SendMessage,
        Room_GetRoomSongId,
        Room_Sync,
    }

    public enum ServerOperate
    {
        Message = 0,
        GameStart = 1,
        RoomClosed = 2,
        UpdateSong = 3,
        ServerClosed
    }

    public enum SongType
    {
        rep = 0,
        Phizone = 1,
        empty = 2
    }
}