using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using Network.Multiplayer.Data;
using Network.Multiplayer.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Multiplayer.Components
{
    public class ServerItem : MonoBehaviour
    {
        private static readonly DateTime TimeStampStart = new(1970, 1, 1, 0, 0, 0, 0);
        private ServerManager serverManager;
        private int id;
        [SerializeField] private Image iBackground, iIcon, iBorder; // TODO: Sky暂时没写图标
        [SerializeField] private Text serverName, tServerId, tServerPing, tServerMotd;
        private string serverUrl;
        private bool online = true, chart;
        private Socket socket;

        public void Init(int id, ServerManager serverManager, string serverUrl, string customName)
        {
            this.id = id;
            this.serverManager = serverManager;
            this.serverUrl = serverUrl;
            serverName.text = customName;
            Refresh();
        }

        public async void Refresh()
        {
            await UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                tServerId.text = "";
                tServerPing.text = "Ping: -ms, 谱面服务: 未知";
                tServerMotd.text = "正在连接服务器...";
                if (!General.TryParseHost(serverUrl, out IPEndPoint endPoint, out _))
                {
                    tServerMotd.text = "<color=red>错误：地址不合法</color>";
                    return;
                }

                await UniTask.SwitchToThreadPool();

                socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Connect(endPoint);
                }
                catch (SocketException)
                {
                    await UniTask.SwitchToMainThread();
                    tServerMotd.text = "<color=red>错误：无法连接至服务器</color>";
                    return;
                }

                await UniTask.SwitchToMainThread();
                tServerMotd.text = "正在获取服务器信息...";
                await UniTask.SwitchToThreadPool();
                long currentTime = (long)Math.Round((DateTime.UtcNow - TimeStampStart).TotalMilliseconds);
                socket.Send(new SendData());
                JObject[] packs = socket.Receive();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while ((packs = socket.Receive()) == null)
                {
                    // await UniTask.WaitForEndOfFrame(this);
                    if (stopwatch.ElapsedMilliseconds < 5000) continue;
                    stopwatch.Stop();
                    await UniTask.SwitchToMainThread();
                    tServerMotd.text = "<color=red>错误：服务器连接超时</color>";
                    await UniTask.SwitchToThreadPool();
                    socket.Close();
                    socket = null;
                    return;
                }

                long receivedTime = (long)Math.Round((DateTime.UtcNow - TimeStampStart).TotalMilliseconds);
                stopwatch.Stop();

                socket.Close();
                socket = null;
                await UniTask.SwitchToMainThread();
                if (packs.Length == 0)
                {
                    tServerMotd.text = "<color=red>错误：服务器语法错误，请联系服务器管理员与服务器开发者</color>";
                    return;
                }

                PingReceiveData data = packs[0].ToObject<PingReceiveData>();
                if (data is not { Status: true })
                {
                    tServerMotd.text = "<color=red>？？？？？？？？？？？？？？？？？？？？？？</color>";
                    return;
                }

                tServerId.text = "@" + data.Name;
                tServerMotd.text = data.Motd;
                tServerPing.text =
                    $"Ping: {receivedTime - currentTime}ms, 谱面服务: {(data.EnableChartUpload ? "在线" : "离线")}";
                online = data.IsOnline;
                chart = data.EnableChartUpload;
            });
        }

        public void OnClicked()
        {
            serverManager.UpdateSelectedServer(id);
        }

        public (string, bool, bool) GetInfo()
        {
            return (serverUrl, online, chart);
        }

        public void SetSelectState(bool state)
        {
            iBorder.color = state ? new Color(0.4f, 0.4f, 0.4f) : new Color(1, 1, 1, 0);
        }
    }

    public class SendData
    {
        [JsonProperty("operate")] public string operate = "Ping";
        [JsonProperty("addition")] public Dictionary<string, string> Addition = new();
    }
}