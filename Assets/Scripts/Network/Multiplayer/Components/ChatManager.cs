using System;
using System.Collections.Generic;
using System.Globalization;
using Network.Multiplayer.Data;
using Network.Multiplayer.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Multiplayer.Components
{
    public class ChatManager : MonoBehaviour
    {
        private static readonly string[] GeneralErrorMessages = { "未连接服务器", "无法发送数据包", "你小子没登录" };
        private readonly object threadLock = new();
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] private GameObject chatMessagePrefab;
        [SerializeField] private InputField ifMessage;
        [SerializeField] private Button bSend;
        private Text tSendButton;
        private List<Message> messages = new List<Message>();

        private void Awake()
        {
            tSendButton = bSend.transform.GetChild(0).gameObject.GetComponent<Text>();
            ifMessage.onEndEdit.AddListener(input => ifMessage.text = input.TrimEnd());
            OnInitOrRoomClosed();
            SocketManager.OnCloseRoomSucceeded += OnInitOrRoomClosed;
            SocketManager.OnQuitRoomSucceeded += OnInitOrRoomClosed;
            SocketManager.OnCreateRoomSucceeded += OnRoomJoinedOrCreated;
            SocketManager.OnJoinRoomSucceeded += OnRoomJoinedOrCreated;
            SocketManager.OnSendMessageSucceeded += () => ifMessage.text = "";
        }

        private void Start()
        {
            Update();
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Return) || ifMessage.isFocused) return;
            ifMessage.ActivateInputField();
            bSend.onClick.Invoke();
        }

        public void AddMessage(string from, string message, MessageType type)
        {
            lock (threadLock)
            {
                messages.Add(new Message(from, message, type));
                GameObject obj = Instantiate(chatMessagePrefab, contentPanel);
                obj.GetComponent<ChatMessage>().Init(from, message, type);
                // LayoutRebuilder.ForceRebuildLayoutImmediate(obj.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
            }
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

        public void RevertChatHistory()
        {
            lock (threadLock)
            {
                foreach (Message message in messages)
                {
                    AddMessage(message.from, message.message, message.messageType);
                }
            }
        }

        private void OnInitOrRoomClosed()
        {
            tSendButton.text = "加入\n房间";
            ifMessage.text = "";
            bSend.onClick.RemoveAllListeners();
            bSend.onClick.AddListener(JoinRoom);
        }

        private void OnRoomJoinedOrCreated()
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
            if (new StringInfo(ifMessage.text).LengthInTextElements > 233)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "信息长度过长", () => { }, "确定");
                return;
            }
            GeneralListener(() => SocketManager.SendRoomMessage(ifMessage.text), GeneralErrorMessages);
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
            AddMessage("Server",  "客户端错误：" + errorMessages[-state - 1], MessageType.Error);
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