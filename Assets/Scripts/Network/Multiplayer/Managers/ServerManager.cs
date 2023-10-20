using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Network.Multiplayer.Components;
using Network.Multiplayer.Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Multiplayer.Managers
{
    public class ServerManager : MonoBehaviour
    {
        private ServerList internalServerList, externalServerList;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] private GameObject serverItemPrefab;
        [SerializeField] private GameObject[] contents;
        [SerializeField] private ServerPanelItem[] buttons;
        [SerializeField] private MPServerTest mpServerTest;
        [SerializeField] private Text serverConnectState;
        [SerializeField] private GameObject serverConnectStatePanel;
        [SerializeField] private GameObject serverConnectStateButtonPanel;
        [SerializeField] private GameObject serverConnectStateFailedButton;
        private List<ServerItem> internalItems = new List<ServerItem>(), externalItems = new List<ServerItem>();
        private int selectedServerId = -1;
        private bool selectedServerIsInternal = false;

        private void Awake()
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                var item = buttons[i];
                if (i == 1) item.Init(this, () => selectedServerId >= 0); // MainPanel的Edit
                else item.Init(this);
            }

            SocketManager.OnLoginSucceeded += () =>
            {
                gameObject.SetActive(false);
                mpServerTest.UpdateConnectState("登录成功");
            };
        }

        private void Start()
        {
            internalServerList =
                JsonConvert.DeserializeObject<ServerList>(Resources.Load<TextAsset>("ServerList").text);
            externalServerList =
                JsonConvert.DeserializeObject<ServerList>(PlayerPrefs.GetString("server_list", "{\"Servers\": []}"));
            if (SocketManager.GetToken() == "") RefreshServerList();
        }

        public void RefreshServerStatus()
        {
            foreach (ServerItem item in internalItems)
            {
                item.Refresh();
            }
            foreach (ServerItem item in externalItems)
            {
                item.Refresh();
            }
        }

        private void RefreshServerList()
        {
            for (int i = 0; i < contentPanel.childCount; i++)
            {
                Destroy(contentPanel.GetChild(i).gameObject);
            }
            internalItems.Clear();
            externalItems.Clear();

            for (int i = 0; i < internalServerList.servers.Count; i++)
            {
                var server = internalServerList.servers[i];
                GameObject item = Instantiate(serverItemPrefab, contentPanel);
                ServerItem serverItem = item.GetComponent<ServerItem>();
                internalItems.Add(serverItem);
                serverItem.Init(i, this, server.url, server.customName, true);
                serverItem.SetSelectState(selectedServerId == i && selectedServerIsInternal);
            }
            for (var i = 0; i < externalServerList.servers.Count; i++)
            {
                var server = externalServerList.servers[i];
                GameObject item = Instantiate(serverItemPrefab, contentPanel);
                ServerItem serverItem = item.GetComponent<ServerItem>();
                externalItems.Add(serverItem);
                serverItem.Init(i, this, server.url, server.customName, false);
                serverItem.SetSelectState(selectedServerId == i && !selectedServerIsInternal);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
        }

        public void UpdateSelectedServer(int id, bool isInternal)
        {
            if (selectedServerId == id && selectedServerIsInternal == isInternal)
            {
                var (url, online, chart) = (isInternal ? internalItems : externalItems)[id].GetInfo();
                Task.Run(() => Connect(url, online, chart));
                return;
            }
            
            selectedServerId = id;
            selectedServerIsInternal = isInternal;
            for (int i = 0; i < internalItems.Count; i++)
            {
                var item = internalItems[i];
                item.SetSelectState(i == selectedServerId && selectedServerIsInternal);
            }
            for (var i = 0; i < externalItems.Count; i++)
            {
                var item = externalItems[i];
                item.SetSelectState(i == selectedServerId && !selectedServerIsInternal);
            }
        }

        private int serverStateSelection = 0;

        private async UniTask Connect(string url, bool online, bool chart)
        {
            await UniTask.SwitchToMainThread();
            serverConnectStatePanel.SetActive(true);
            serverConnectStateButtonPanel.SetActive(false);
            serverConnectStateFailedButton.SetActive(false);
            mpServerTest.UpdateConnectState("连接中...");
            serverConnectState.text = "正在连接服务器...";
            await new WaitForSeconds(0.3f); // 让玩家看着更真实一点
            if (!online)
            {
                serverConnectState.text = "警告：服务器未启用正版验证，这可能带来风险，是否继续？";
                SetupServerStateSelect();
                while (serverStateSelection == 0)
                {
                    await UniTask.WaitForEndOfFrame(this);
                }

                if (serverStateSelection == -1)
                {
                    serverConnectStatePanel.SetActive(false);
                    return;
                }

                serverConnectState.text = "正在连接服务器...";
            }

            await UniTask.SwitchToTaskPool();
            int state = SocketManager.CreateSocket(url);
            await UniTask.SwitchToMainThread();
            if (state == 0)
            {
                mpServerTest.UpdateConnectState("正在登录...");
                SocketManager.Set(online, chart);
                await UniTask.SwitchToTaskPool();
                int state1 = SocketManager.Login();
                await UniTask.SwitchToMainThread();
                if (state1 == 0)
                {
                    mpServerTest.UpdateConnectState("成功发送登录数据");
                    return;
                }
                mpServerTest.UpdateConnectState(serverConnectState.text = "连接失败：" + state1 switch
                {
                    -1 => "未知错误",
                    -2 => "无法连接至服务器",
                    -3 => "已经登录",
                    _ => throw new ArgumentOutOfRangeException(nameof(state1), state1, "Unknown Exception")
                });
                serverConnectStateFailedButton.SetActive(true);
                return;
            }

            mpServerTest.UpdateConnectState(serverConnectState.text = "连接失败：" + state switch
            {
                -1 => "未知错误",
                -2 => "服务器Url不合法",
                -3 => "无法连接服务器，可能是未启动",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown Exception")
            });
            serverConnectStateFailedButton.SetActive(true);
        }

        private void SetupServerStateSelect()
        {
            serverStateSelection = 0;
            serverConnectStateButtonPanel.SetActive(true);
        }

        public void SetServerStateSelection(int state)
        {
            serverStateSelection = state;
            serverConnectStateButtonPanel.SetActive(false);
        }

        public void UpdatePanel(int id)
        {
            for (int i = 0; i < contents.Length; i++)
            {
                contents[i].SetActive(i == id);
            }
        }

        // 添加
        [SerializeField] private InputField ifName, ifUrl;

        public void CleanField()
        {
            ifName.text = "";
            ifUrl.text = "";
        }

        public void AddServer()
        {
            externalServerList.servers.Add(new Server
            {
                customName = ifName.text,
                url = ifUrl.text
            });
            PlayerPrefs.SetString("server_list", JsonConvert.SerializeObject(externalServerList));
            PlayerPrefs.Save();
            RefreshServerList();
        }

        public void OnValidateUrl(string text)
        {
            if (text.Trim() == ifUrl.text) return;
            ifUrl.text = text.Trim();
        }

        // 编辑
        [SerializeField] private InputField ifName1, ifUrl1;

        public void OnStartEdit()
        {
            if (selectedServerId < 0) return;
            ifName1.text = externalServerList.servers[selectedServerId].customName;
            ifUrl1.text = externalServerList.servers[selectedServerId].url;
        }

        public void CleanField1()
        {
            ifName1.text = "";
            ifUrl1.text = "";
        }

        public void EditServer()
        {
            if (selectedServerId < 0) return;
            externalServerList.servers = externalServerList.servers.Select((server, i) =>
            {
                if (i != selectedServerId) return server;
                server.customName = ifName1.text;
                server.url = ifUrl1.text;
                return server;
            }).ToList();
            PlayerPrefs.SetString("server_list", JsonConvert.SerializeObject(externalServerList));
            PlayerPrefs.Save();
            RefreshServerList();
        }

        public void OnValidateUrl1(string text)
        {
            if (text.Trim() == ifUrl1.text) return;
            ifUrl1.text = text.Trim();
        }

        // 删除
        public void DeleteServer()
        {
            if (selectedServerId < 0) return;
            externalServerList.servers.RemoveAt(selectedServerId);
            selectedServerId = -1;
            PlayerPrefs.SetString("server_list", JsonConvert.SerializeObject(externalServerList));
            PlayerPrefs.Save();
            RefreshServerList();
        }
    }
}