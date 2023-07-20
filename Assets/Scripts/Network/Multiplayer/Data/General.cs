using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Network.Multiplayer.Data
{
    public static class General
    {
        private static readonly UTF8Encoding NoBomUtf8Encoding = new(false);

        public static int Send(this Socket socket, object? value)
        {
            string messageToSend = JsonConvert.SerializeObject(value);
            Debug.Log("尝试发送" + messageToSend);
            return socket.Send(NoBomUtf8Encoding.GetBytes(messageToSend));
        }

        public static JObject[]? Receive(this Socket socket)
        {
            if (socket.Available <= 0) return null;
            byte[] buffer = new byte[8];
            List<byte> dataList = new List<byte>();
            int length;
            while ((length = socket.Receive(buffer)) > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    dataList.Add(buffer[i]);
                }

                if (socket.Available <= 0 || length < buffer.Length) break;
            }

            if (dataList.Count == 0) throw new NullReferenceException("Received Nothing");

            string s = NoBomUtf8Encoding.GetString(dataList.ToArray());
            return SplitSocketPacks(s);
        }

        private static JObject[] SplitSocketPacks(string s)
        {
            List<JObject> packs = new List<JObject>();
            bool isInString = false;
            int j = 0;
            TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(s);
            StringBuilder stringBuilder = new StringBuilder();
            while (textElementEnumerator.MoveNext())
            {
                string element = textElementEnumerator.GetTextElement();
                if (stringBuilder.Length == 0 && element != "{") throw new ArgumentException("你这包有问题啊");
                if (element == "\"") isInString = !isInString;
                else if (!isInString)
                {
                    if (element == "{") j++;
                    if (element == "}") j--;
                }

                stringBuilder.Append(element);
                if (j != 0) continue;
                string s1 = stringBuilder.ToString();
                packs.Add(JObject.Parse(s1));
                stringBuilder.Clear();
            }
            return packs.ToArray();
        }

        public static void Print(this Exception exception)
        {
            Debug.Log(exception.Message + "\n" + exception.StackTrace);
        }
    }

    public class GeneralSendData<T>
    {
        [JsonProperty("operate")] public string Operate;
        [JsonProperty("username")] public string Username;
        [JsonProperty("addition")] public Dictionary<string, T> Addition = new();
    }

    public class LoginSendData<T> : GeneralSendData<T>
    {
        [JsonProperty("verifyToken")] public string VerifyToken;
    }

    public class SendDataWithToken<T> : GeneralSendData<T>
    {
        [JsonProperty("loginToken")] public string LoginToken;
    }
    
    public class GeneralReceiveData
    {
        [JsonProperty("Type")] public string Type;
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

    public class NewMessageActiveReceiveData : ActiveReceiveData
    {
        [JsonProperty("from")] public string Author;
        [JsonProperty("message")] public string Message;
    }

    public enum ClientOperate
    {
        LoginToServer = 0,
        User_CreateNewRoom = 1,
        User_CloseRoom = 2,
        User_JoinRoom = 3,
        Room_UpdateSong = 4,
        Room_GameStart = 5,
        Room_SendMessage = 6,
        Room_GetRoomSongId = 7,
        Room_GetRoomInfo = 8
    }

    public enum ServerOperate
    {
        NewMessage = 0,
        GameStart = 1,
        RoomClosed = 2,
        UpdateSong = 3
    }
}