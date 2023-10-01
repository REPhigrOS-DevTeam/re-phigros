using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MainCore.Common;
using Network.Multiplayer.Components;
using Network.Multiplayer.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Multiplayer.Managers
{
    public class ChatManager : MonoBehaviour
    {
        private static readonly string[] GeneralErrorMessages = { "未连接服务器", "无法发送数据包", "你小子没登录" };
        private static readonly object threadLock = new();
        [SerializeField] private ScrollRect scrollView;
        private RectTransform contentPanel;
        [SerializeField] private GameObject chatMessagePrefab;
        [SerializeField] private InputField ifMessage;
        [SerializeField] private Button bSend;
        private RectTransform scrollViewTransform;
        private Text tSendButton;
        private static List<Message> messages = new List<Message>();
        public static ChatManager Instance;
        private static bool inited;

        private void Awake()
        {
            contentPanel = scrollView.content;
            tSendButton = bSend.transform.GetChild(0).gameObject.GetComponent<Text>();
            ifMessage.onEndEdit.AddListener(input => ifMessage.text = input.TrimEnd());
            scrollViewTransform = (RectTransform)scrollView.transform;
            OnInitOrRoomClosed();
            // SocketManager.OnSendMessageSucceeded += () => ifMessage.text = "";
            if (!inited)
            {
                inited = true;
                SceneTransit.OnSceneClosing += () => Instance = null;
            }

            Instance = this;
        }

        private void Start()
        {
            Update();
        }

        private void Update()
        {
            if (SocketManager.GetToken() == "" ||
                (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter)) ||
                ifMessage.isFocused) return;
            ifMessage.ActivateInputField();
            bSend.onClick.Invoke();
        }

        public static void AddMessage(string from, string message, MessageType type)
        {
            lock (threadLock)
            {
                messages.Add(new Message(from, message, type));
                if (Instance)
                {
                    Instance.AddMessageInternal(from, message, type);
                }
                else
                {
                    GameObject o = GameObject.FindWithTag("ChatManager");
                    if (!o) return;
                    Instance = o.GetComponent<ChatManager>();
                    Instance.AddMessageInternal(from, message, type);
                }
            }
        }

        private async void AddMessageInternal(string from, string message, MessageType type)
        {
            bool autoScroll = scrollView.verticalNormalizedPosition < 0.01f ||
                              scrollView.content.sizeDelta.y <= scrollViewTransform.sizeDelta.y;
            GameObject obj = Instantiate(chatMessagePrefab, contentPanel);
            obj.GetComponent<ChatMessage>().Init(from, message, type);
            // LayoutRebuilder.ForceRebuildLayoutImmediate(obj.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
            await UniTask.Yield();
            if (scrollView.content.sizeDelta.y < scrollViewTransform.sizeDelta.y) scrollView.verticalNormalizedPosition = 1f;
            else if (autoScroll) scrollView.verticalNormalizedPosition = 0f;
        }

        public void CleanChatHistory()
        {
            lock (threadLock)
            {
                messages.Clear();
                Transform t;
                for (int i = 0; i < contentPanel.childCount; i++)
                {
                    t = contentPanel.GetChild(i);
                    Destroy(t.gameObject);
                }
            }
        }

        public async void RevertChatHistory()
        {
            await Task.Run(async () =>
            {
                await new WaitUntil(() => Instance != null);
                lock (threadLock)
                {
                    foreach (Message message in messages)
                    {
                        AddMessageInternal(message.from, message.message, message.messageType);
                    }
                }
            });
        }

        public void OnInitOrRoomClosed()
        {
            tSendButton.text = "加入\n房间";
            ifMessage.text = "";
            bSend.onClick.RemoveAllListeners();
            bSend.onClick.AddListener(JoinRoom);
        }

        public void OnRoomJoinedOrCreated()
        {
            tSendButton.text = "发送\n信息";
            ifMessage.text = "";
            bSend.onClick.RemoveAllListeners();
            bSend.onClick.AddListener(SendMessage);
        }

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(ifMessage.text))
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "信息为空", () => { }, "确定");
                return;
            }

            // if (new StringInfo(ifMessage.text).LengthInTextElements > 233)
            // {
            //     InGameUIManager.ShowModalWindowWithClose("错误", "信息长度过长", () => { }, "确定");
            //     return;
            // }

            GeneralListener(() => SocketManager.SendRoomMessage(ifMessage.text), GeneralErrorMessages);
            ifMessage.text = "";
        }

        private void JoinRoom()
        {
            if (int.TryParse(ifMessage.text, out int i))
            {
                GeneralListener(() => SocketManager.JoinRoom(i + ""), GeneralErrorMessages);
            }
            else
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "房间id不合法", () => { }, "确定");
            }
        }

        private void GeneralListener(Func<int> getState, params string[] errorMessages)
        {
            int state = getState.Invoke();
            if (state == 0) return;
            AddMessage("Server", "客户端错误：" + errorMessages[-state - 1], MessageType.Error);
        }

        public class Message
        {
            public string from;
            public string message;
            public MessageType messageType;

            public Message(string from, string message, MessageType messageType)
            {
                this.from = from;
                this.message = message;
                this.messageType = messageType;
            }
        }
    }
}