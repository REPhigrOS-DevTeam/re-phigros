using System;
using System.Collections.Generic;
using System.Linq;
using Network.Multiplayer.Components;
using Network.Multiplayer.Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Multiplayer.Managers
{
    public class ServerManager : MonoBehaviour
    {
        private ServerList serverList;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] private GameObject serverItemPrefab;
        [SerializeField] private GameObject[] contents;
        [SerializeField] private ServerPanelItem[] buttons;
        private List<ServerItem> items = new List<ServerItem>();
        private int selectedServerId = -1;

        private void Awake()
        {
            foreach (ServerPanelItem item in buttons)
            {
                item.Init(this);
            }
            serverList = JsonConvert.DeserializeObject<ServerList>(PlayerPrefs.GetString("server_list", "{\"Servers\": []}"));
        }

        private void Start()
        {
            RefreshServerList();
        }

        public void RefreshServerStatus()
        {
            foreach (ServerItem item in items)
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
            for (var i = 0; i < serverList.servers.Count; i++)
            {
                var server = serverList.servers[i];
                GameObject item = Instantiate(serverItemPrefab, contentPanel);
                ServerItem serverItem = item.GetComponent<ServerItem>();
                items.Add(serverItem);
                serverItem.Init(i, this, server.url, server.customName);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
        }
        public void UpdateSelectedServer(int id)
        {
            if (selectedServerId == id)
            {
                var (url, online, chart) = items[id].GetInfo();
                Connect(url, online, chart);
                return;
            }
            selectedServerId = id;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.SetSelectState(i == id);
            }
        }

        private void Connect(string url, bool online, bool chart)
        {
            
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
            serverList.servers.Add(new Server
            {
                customName = ifName.text,
                url = ifUrl.text
            });
            PlayerPrefs.SetString("server_list", JsonConvert.SerializeObject(serverList));
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
            ifName1.text = serverList.servers[selectedServerId].customName;
            ifUrl1.text = serverList.servers[selectedServerId].url;
        }
        
        public void CleanField1()
        {
            ifName1.text = "";
            ifUrl1.text = "";
        }

        public void EditServer()
        {
            if (selectedServerId < 0) return;
            serverList.servers = serverList.servers.Select((server, i) =>
            {
                if (i != selectedServerId) return server;
                server.customName = ifName1.text;
                server.url = ifUrl1.text;
                return server;
            }).ToList();
            PlayerPrefs.SetString("server_list", JsonConvert.SerializeObject(serverList));
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
            serverList.servers.RemoveAt(selectedServerId);
            selectedServerId = -1;
            PlayerPrefs.SetString("server_list", JsonConvert.SerializeObject(serverList));
            PlayerPrefs.Save();
            RefreshServerList();
        }
    }
}