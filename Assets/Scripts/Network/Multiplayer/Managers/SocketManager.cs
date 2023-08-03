using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Threading;
using JetBrains.Annotations;
using MainCore.Utilities;
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
        private static string songId = "";
        private static string tryJoinRoomId = "";
        private static Task taskReceive = null;
        private static JObject[] packs;
        private static bool stopReceive = false;
        private static ClientOperate currentClientOperate = ClientOperate.User_LoginToServer;
        private static bool isInited = false;
        private static ChatManager chatManager;
        private static object threadLock = new();

        public static Action<int> OnUpdateSongReceived = _ => { };

        public static Action<ClientOperate>
            OnSendPrepared = _ => { },
            OnBackReceived = _ => { };

        public static Action
            OnConnecting = () => { },
            OnConnectSucceeded = () => { },
            OnConnectFailed = () => { },
            OnDisconnect = () => { },
            OnLoginSucceeded = () => { },
            OnCreateRoomSucceeded = () => { },
            OnCloseRoomSucceeded = () => { },
            OnJoinRoomSucceeded = () => { },
            OnQuitRoomSucceeded = () => { },
            OnUpdateSongSucceeded = () => { },
            OnStartGameSucceeded = () => { },
            OnSendMessageSucceeded = () => { },
            OnGetRoomSongIdSucceeded = () => { },
            OnGetRoomInfoSucceeded = () => { };

        public static void Init(ChatManager chatManager)
        {
            if (isInited) return;
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
            // if (socket.Connected) return -1; // 已连接
            if (!General.TryParseHost(serverUrl, out IPEndPoint endPoint)) return -2; // 地址不合法
            try
            {
                socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                OnConnecting.Invoke();
                socket.Connect(endPoint);
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

        #region ClientOperations

        public static int Login()
        {
            if (!socket.Connected) return -1; // 未连接
            if (token != "") return -3; // 已登录
            try
            {
                currentClientOperate = ClientOperate.User_LoginToServer;
                OnSendPrepared.Invoke(currentClientOperate);
                LoginSendData pack = new LoginSendData
                {
                    Operate = currentClientOperate.ToString(),
                    Username = RepAPI.Username,
                    VerifyToken = RepAPI.VerifyToken
                };
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
            if (!socket.Connected) return -1; // 未连接
            if (token == "") return -3; // 未登录
            return GeneralSend(ClientOperate.User_CreateNewRoom,
                GetSendDataWithToken());
        }

        public static int CloseRoom()
        {
            if (!socket.Connected) return -1; // 未连接
            if (token == "") return -3; // 未登录
            return GeneralSend(ClientOperate.User_CloseRoom,
                GetSendDataWithToken());
        }

        public static int JoinRoom(string roomId)
        {
            if (!socket.Connected) return -1; // 未连接
            if (token == "") return -3; // 未登录
            tryJoinRoomId = roomId;
            SendDataWithToken pack = GetSendDataWithToken();
            pack.Addition.Add("RoomID", roomId);
            return GeneralSend(ClientOperate.User_JoinRoom, pack);
        }

        public static int QuitRoom()
        {
            if (!socket.Connected) return -1; // 未连接
            if (token == "") return -3; // 未登录
            return GeneralSend(ClientOperate.User_QuitRoom, GetSendDataWithToken());
        }

        public static int SendRoomMessage(string msg)
        {
            if (!socket.Connected) return -1; // 未连接
            if (token == "") return -3; // 未登录
            SendDataWithToken pack = GetSendDataWithToken();
            pack.Addition.Add("Message", msg);
            return GeneralSend(ClientOperate.Room_SendMessage, pack);
        }

        public static int StartGame()
        {
            if (!socket.Connected) return -1; // 未连接
            if (token == "") return -3; // 未登录
            return GeneralSend(ClientOperate.Room_GameStart, GetSendDataWithToken());
        }

        public static void LeaveServer()
        {
            lock (threadLock)
            {
                if (!socket.Connected) return; // 未连接
                if (string.IsNullOrEmpty(token)) return; // 未登录
                try
                {
                    var value = GetSendDataWithToken();
                    currentClientOperate = ClientOperate.User_LeaveServer;
                    value.Operate = ClientOperate.User_LeaveServer.ToString();
                    socket.Send(value);
                    Thread.Sleep(1000);
                    Close();
                }
                catch (SocketException e)
                {
                    e.Print();
                }
            }
        }

        public static int UpdateSong(int songId, SongType type = SongType.rep)
        {
            SendDataWithToken pack = GetSendDataWithToken();
            pack.Addition.Add("songId", songId + "");
            pack.Addition.Add("songType", type.ToString());
            return GeneralSend(ClientOperate.Room_UpdateSong, pack);
        }

        #endregion

        private static int GeneralSend(ClientOperate clientOperate, GeneralSendData value)
        {
            lock (threadLock)
            {
                if (!socket.Connected) return -1; // 未连接
                if (string.IsNullOrEmpty(token)) return -3; // 未登录
                try
                {
                    currentClientOperate = clientOperate;
                    OnSendPrepared.Invoke(currentClientOperate);
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

        private static SendDataWithToken GetSendDataWithToken()
        {
            SendDataWithToken pack = new SendDataWithToken
            {
                Username = RepAPI.Username,
                LoginToken = token
            };
            return pack;
        }

        private static int StartReceive()
        {
            if (!socket.Connected) return -1; // 未连接
            if (taskReceive != null) return -2; // 已开启Receive
            stopReceive = false;
            taskReceive = Task.Run(() =>
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
            OnBackReceived.Invoke(clientOperate);
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
                        }, printOnSuccess: true);
                    break;
                case ClientOperate.User_CloseRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法关闭房间");
                    break;
                case ClientOperate.User_JoinRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法加入房间", callback: _ =>
                    {
                        OnJoinRoomSucceeded.Invoke();
                        roomId = tryJoinRoomId;
                    }, printOnSuccess: true);
                    break;
                case ClientOperate.Room_UpdateSong:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法更换谱面", callback: _ => OnUpdateSongSucceeded.Invoke());
                    break;
                case ClientOperate.Room_GameStart:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法开始游戏", callback: _ => OnStartGameSucceeded.Invoke());
                    break;
                case ClientOperate.Room_SendMessage:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法发送信息", callback: _ => OnSendMessageSucceeded.Invoke());
                    break;
                case ClientOperate.Room_GetRoomSongId:
                    DealWithMsg<GetSongIdReceive>(pack, "错误：无法获取歌曲id",
                        callback: _ => OnGetRoomSongIdSucceeded.Invoke());
                    break;
                case ClientOperate.Room_GetRoomInfo:
                    DealWithMsg<RoomInfoReceive>(pack, "错误：无法获取房间信息", callback: _ => OnGetRoomInfoSucceeded.Invoke());
                    break;
                case ClientOperate.User_QuitRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法退出房间", callback: _ => OnQuitRoomSucceeded.Invoke(),
                        printOnSuccess: true);
                    break;
                case ClientOperate.User_Ready:
                    break;
                case ClientOperate.User_UnReady:
                    break;
                case ClientOperate.Room_UserGameEnd:
                    break;
                case ClientOperate.Room_UserQuitGame:
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
            bool printOnSuccess = false)
            where T : BackReceiveData
        {
            T? received = jObject.ToObject<T>();
            if (received == null) throw new ArgumentException("Unknown Exception");
            Debug.Log("成功接收到数据：" + JsonConvert.SerializeObject(received, Formatting.None));
            string serializeObject = JsonConvert.SerializeObject(received);
            if (!received.Status)
            {
                chatManager.AddMessage("Server", errorMessage + "——" + received.Message, MessageType.Error);
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
                if (printOnSuccess) chatManager.AddMessage("Server", received.Message, MessageType.Server);
            }
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
                case ServerOperate.Message:
                    MessageActiveReceive receive = pack.ToObject<MessageActiveReceive>();
                    if (receive == null) throw new ArgumentException("吃席");
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
                    roomId = "";
                    chatManager.AddMessage("Server", "房间已关闭", MessageType.Server);
                    OnCloseRoomSucceeded.Invoke();
                    break;
                case ServerOperate.UpdateSong:
                    Debug.Log("试图更新歌曲信息：" +
                              JsonConvert.SerializeObject(pack.ToObject<UpdaeSongActiveReceive>(), Formatting.None));
                    OnUpdateSongReceived.Invoke(int.Parse(pack["songId"].ToString()));
                    break;
                case ServerOperate.ServerClosed:
                    Util.QuitApp();
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

        private static void OnRoomQuited()
        {
            roomId = "";
            chatManager.AddMessage("Server", "已退出房间", MessageType.Server);
            OnQuitRoomSucceeded.Invoke();
        }

        private static void Close()
        {
            if (socket == null) return;
            stopReceive = true;
            taskReceive = null;
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