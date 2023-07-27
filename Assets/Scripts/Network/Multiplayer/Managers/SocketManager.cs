using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading.Tasks;
using Baracuda.Threading;
using JetBrains.Annotations;
using Network.Multiplayer.Components;
using Network.Multiplayer.Data;
using Network.Verify.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Utilities;
using MessageType = Network.Multiplayer.Data.MessageType;

namespace Network.Multiplayer.Managers
{
    public static class SocketManager
    {
        private static Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static string token = "";
        private static string roomId = "";
        private static string tryJoinRoomId = "";
        private static Task receiveTask = null;
        private static JObject[] packs;
        private static bool stopReceive = false;
        private static ClientOperate currentClientOperate = ClientOperate.User_LoginToServer;
        private static bool isInited = false;
        private static ChatManager chatManager;
        private static object threadLock = new();
        public static RoomState state;

        public static Action
            OnSendPrepared = () => { },
            OnBackReceived = () => { },
            OnConnecting = () => { },
            OnConnectSucceeded = () => { },
            OnConnectFailed = () => { },
            OnDisconnect = () => { },
            OnLoginSucceeded = () => { },
            OnCreateRoomSucceeded = () => { },
            OnJoinRoomSucceeded = () => { },
            OnCloseRoomSucceeded = () => { },
            OnUpdateSongSucceeded = () => { },
            OnStartGameSucceeded = () => { },
            OnSendMessageSucceeded = () => { },
            OnGetRoomSongIdSucceeded = () => { },
            OnGetRoomInfoSucceeded = () => { };

        public static void Init(ChatManager chatManager)
        {
            SocketManager.chatManager = chatManager;
            isInited = true;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += playModeState =>
            {
                if (playModeState != PlayModeStateChange.ExitingPlayMode) return;
                LeaveServer();
            };
#else
            Application.quitting += LeaveServer;
            Application.wantsToQuit += () => true;
#endif
        }
        public static int CreateSocket(string serverUrl)
        {
            Close();
            socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // if (socket.Connected) return -1; // 已连接
            var strings = serverUrl.Split(":");
            if (strings.Length != 2 || !int.TryParse(strings[1], out int port)) return -2; // 地址不合法
            try
            {
                OnConnecting.Invoke();
                socket.Connect(strings[0], port);
                OnConnectSucceeded.Invoke();
                StartReceive();
                return 0;
            }
            catch (SocketException e)
            {
                e.Print();
                OnConnectFailed.Invoke();
                return -3; // 无法连接
            }
        }

        public static int Login()
        {
            if (!socket.Connected) return -1; // 未连接
            if (token != "") return -3;
            try
            {
                OnSendPrepared.Invoke();
                currentClientOperate = ClientOperate.User_LoginToServer;
                LoginSendData<string> pack = new LoginSendData<string>
                {
                    Operate = currentClientOperate.ToString(),
                    Username = RepAPI.Username,
                    VerifyToken = "" // TODO: 输入rep账户系统的token
                };
                pack.VerifyToken = RepAPI.VerifyToken;
                socket.Send(pack);
                return 0;
            }
            catch (SocketException e)
            {
                e.Print();
                return -2; // 无法发送
            }
        }

        public static int CreateRoom()
        {
            return GeneralSend(ClientOperate.User_CreateNewRoom,
                GetSendDataWithToken<string>());
        }

        public static int JoinRoom(string roomId)
        {
            tryJoinRoomId = roomId;
            SendDataWithToken<string> pack = GetSendDataWithToken<string>();
            pack.Addition.Add("RoomID", roomId);
            return GeneralSend(ClientOperate.User_JoinRoom, pack);
        }

        public static int CloseRoom()
        {
            return GeneralSend(ClientOperate.User_CloseRoom,
                GetSendDataWithToken<string>());
        }

        public static int SendRoomMessage(string msg)
        {
            SendDataWithToken<string> pack = GetSendDataWithToken<string>();
            pack.Addition.Add("NewMessage", msg);
            return GeneralSend(ClientOperate.Room_SendMessage, pack);
        }

        public static int StartGame()
        {
            return GeneralSend(ClientOperate.Room_GameStart, GetSendDataWithToken<string>());
        }

        public static void LeaveServer()
        {
            lock (threadLock)
            {
                if (!socket.Connected) return; // 未连接
                if (string.IsNullOrEmpty(token)) return; // 未登录
                try
                {
                    var value = GetSendDataWithToken<string>();
                    currentClientOperate = ClientOperate.User_LeaveServer;
                    value.Operate = ClientOperate.User_LeaveServer.ToString();
                    socket.Send(value);
                    Close();
                }
                catch (SocketException e)
                {
                    e.Print();
                }
            }
        }

        private static int GeneralSend<T>(ClientOperate clientOperate, GeneralSendData<T> value)
        {
            lock (threadLock)
            {
                if (!socket.Connected) return -1; // 未连接
                if (string.IsNullOrEmpty(token)) return -3; // 未登录
                try
                {
                    OnSendPrepared.Invoke();
                    currentClientOperate = clientOperate;
                    value.Operate = clientOperate.ToString();
                    socket.Send(value);
                    return 0;
                }
                catch (SocketException e)
                {
                    e.Print();
                    return -2; // 无法发送
                }
            }
        }

        private static SendDataWithToken<T> GetSendDataWithToken<T>()
        {
            SendDataWithToken<T> pack = new SendDataWithToken<T>
            {
                Username = RepAPI.Username,
                LoginToken = token
            };
            return pack;
        }

        private static int StartReceive()
        {
            if (!socket.Connected) return -1; // 未连接
            if (receiveTask != null) return -2; // 已开启Receive
            stopReceive = false;
            receiveTask = Task.Run(() =>
            {
                while (!stopReceive)
                {
                    packs = socket.Receive();
                    if (packs == null) continue;
                    ClientOperate clientOperate = currentClientOperate;
                    foreach (JObject pack in packs)
                    {
                        Task.Run(() => { Dispatcher.Invoke(AnalyzePack(pack, clientOperate)); });
                    }
                }
            });
            return 0;
        }

        private static IEnumerator AnalyzePack(JObject pack, ClientOperate clientOperate)
        {
            // yield return new WaitWhile(() => InGameUIManager.IsActive);
            if (!pack.ContainsKey("Type"))
            {
                Debug.Log("错误：无法处理接收到的数据");
                Debug.Log(pack);
                yield break;
            }

            if (pack["Type"].ToString() == "Active")
            {
                ExecuteActivePack(pack);
            }
            else
            {
                ExecuteBackPack(pack, clientOperate);
            }
        }

        private static void ExecuteBackPack(JObject pack, ClientOperate clientOperate)
        {
            OnBackReceived.Invoke();
            switch (clientOperate)
            {
                case ClientOperate.User_LoginToServer:
                    DealWithLogin(pack);
                    break;
                case ClientOperate.User_CreateNewRoom:
                    DealWithMsg<CreateRoomReceive>(pack, "错误：无法创建房间",
                        createRoomReceive => createRoomReceive.RoomId != null,
                        createRoomReceive =>
                        {
                            OnCreateRoomSucceeded.Invoke();
                            roomId = createRoomReceive.RoomId;
                        });
                    break;
                case ClientOperate.User_CloseRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法关闭房间", callback: _ => OnRoomClosed());
                    break;
                case ClientOperate.User_JoinRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法加入房间", callback: _ =>
                    {
                        OnJoinRoomSucceeded.Invoke();
                        roomId = tryJoinRoomId;
                    });
                    break;
                case ClientOperate.Room_UpdateSong:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法更换谱面", callback: _ => OnUpdateSongSucceeded.Invoke());
                    break;
                case ClientOperate.Room_GameStart:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法开始游戏", callback: _ => OnStartGameSucceeded.Invoke(),
                        printOnSuccess: false);
                    break;
                case ClientOperate.Room_SendMessage:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法发送信息", callback: _ => OnSendMessageSucceeded.Invoke(),
                        printOnSuccess: false);
                    break;
                case ClientOperate.Room_GetRoomSongId:
                    DealWithMsg<GetSongIdReceive>(pack, "错误：无法获取歌曲id",
                        callback: _ => OnGetRoomSongIdSucceeded.Invoke());
                    break;
                case ClientOperate.Room_GetRoomInfo:
                    DealWithMsg<RoomInfoReceive>(pack, "错误：无法获取房间信息", callback: _ => OnGetRoomInfoSucceeded.Invoke());
                    break;
                case ClientOperate.User_LeaveServer:
                default:
                    throw new ArgumentOutOfRangeException(nameof(clientOperate), clientOperate, null);
            }
        }

        private static void DealWithLogin(JObject jObject)
        {
            string errorMessage = "错误：无法登录";
            LoginReceive? received = jObject.ToObject<LoginReceive>();
            if (received == null) throw new ArgumentException("Unknown Exception");
            Debug.Log("成功接收到数据：" + JsonConvert.SerializeObject(received, Formatting.None));
            string serializeObject = JsonConvert.SerializeObject(received);
            if (!received.Status)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", errorMessage + "\n" + received.Message, () => { }, "确认");
                Debug.Log("错误：无法完成操作\n" + serializeObject);
            }
            else
            {
                if (received.token == null)
                {
                    InGameUIManager.ShowModalWindowWithClose("错误", errorMessage + "\n返回包体有误\n" + serializeObject,
                        () => { }, "确认");
                    Debug.Log("返回的数据有误：\n" + received.Message);
                    return;
                }

                token = received.token;
                OnLoginSucceeded.Invoke();
                InGameUIManager.ShowModalWindowWithClose("提示", received.Message, () => { }, "确认");
            }
        }

        private static void DealWithMsg<T>(JObject jObject,
            string errorMessage, [CanBeNull] Func<T, bool> checkData = null, [CanBeNull] Action<T> callback = null,
            bool printOnSuccess = true)
            where T : BackReceiveData
        {
            T? received = jObject.ToObject<T>();
            if (received == null) throw new ArgumentException("Unknown Exception");
            Debug.Log("成功接收到数据：" + JsonConvert.SerializeObject(received, Formatting.None));
            string serializeObject = JsonConvert.SerializeObject(received);
            if (!received.Status)
            {
                chatManager.AddMessage("Server", errorMessage + "\n" + received.Message, MessageType.Error);
                Debug.Log("错误：无法完成操作\n" + serializeObject);
            }
            else
            {
                if (checkData != null && !checkData.Invoke(received))
                {
                    chatManager.AddMessage("Server", errorMessage + "\n返回包体有误\n" + serializeObject, MessageType.Error);
                    Debug.Log("返回的数据有误：\n" + received.Message);
                    return;
                }

                callback?.Invoke(received);
            }

            if (printOnSuccess) chatManager.AddMessage("Server", received.Message, MessageType.Server);
        }

        private static void ExecuteActivePack(JObject pack)
        {
            Debug.Log("收到Active包：" + pack);
            if (!pack.ContainsKey("operate"))
            {
                Debug.Log("错误：非法Active包\n" + pack);
                return;
            }

            if (!Enum.TryParse(pack["operate"].ToString(), out ServerOperate serverOperate))
            {
                Debug.Log("错误：未知的服务器操作\n" + pack);
                return;
            }

            switch (serverOperate) // TODO: 分析Active包
            {
                case ServerOperate.NewMessage:
                    NewMessageActiveReceive receive = pack.ToObject<NewMessageActiveReceive>();
                    if (receive.IsServer)
                    {
                        switch (receive.Author)
                        {
                            case "joinServer":
                                Debug.Log(receive.Message);
                                break;
                            case "joinRoom":
                                chatManager.AddMessage(receive.Author, receive.Message, MessageType.Server);
                                break;
                            default:
                                chatManager.AddMessage(receive.Author, receive.Message, MessageType.Server);
                                Debug.Log("错误：暂且未知的Server NewMessage operate种类：" + receive.Author);
                                break;
                        }
                    }
                    else
                    {
                        chatManager.AddMessage(receive.Author, receive.Message,
                            receive.Author == RepAPI.Username ? MessageType.Self : MessageType.Common);
                    }

                    break;
                case ServerOperate.GameStart:
                    chatManager.AddMessage("Server", "游戏开始了，但我没做，只是愣着", MessageType.Server);
                    break;
                case ServerOperate.RoomClosed:
                    OnRoomClosed();
                    break;
                case ServerOperate.UpdateSong:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetToken()
        {
            return token;
        }

        public static string GetRoomId()
        {
            return roomId;
        }

        private static void OnRoomClosed()
        {
            roomId = "";
            chatManager.AddMessage("Server", "房间已关闭", MessageType.Server);
            OnCloseRoomSucceeded.Invoke();
        }

        private static void Close()
        {
            if (socket == null) return;
            stopReceive = true;
            receiveTask = null;
            try
            {
                socket.Disconnect(false);
            }
            catch (SocketException)
            {
            }

            socket.Close();
            OnDisconnect.Invoke();
            socket = null;
            token = roomId = tryJoinRoomId = "";
        }
    }
}