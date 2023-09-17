using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using MainCore.Common;
using MainCore.Utilities;
using Network.Account;
using Network.Multiplayer.Components;
using Network.Multiplayer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PhiZone.Data;
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
        private static string serverUrl = "";
        private static string token = "";
        private static string serverId = "";
        private static string roomId = "";
        private static bool isOwner = false;
        private static string songId = "";
        private static SongType songType = SongType.empty;
        private static string tryJoinRoomId = "";
        private static Task taskReceive = null;
        private static JObject[] packs;
        private static bool stopReceive = false;
        private static ClientOperate currentClientOperate = ClientOperate.User_LoginToServer;
        private static bool isInited = false;
        private static object threadLock = new();
        private static bool isReady = false;
        private static bool allGameEnded = true;
        private static bool isOnline = false, enableChart = false;

        public static Action<string, SongType> OnUpdateSongReceived = (_, _) => { };

        public static Action<ClientOperate>
            OnSendPrepared = _ => { },
            OnBackReceived = _ => { };

        public static Action<RoomInfo> OnGetRoomInfoSucceeded = _ => { };

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
            OnGameStarted = () => { },
            OnSendMessageSucceeded = () => { },
            OnGetRoomSongIdSucceeded = () => { };

        public static Action OnGetRoomInfoFailed = () => { };

        public static void Init()
        {
            if (isInited) return;
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
            SceneTransit.OnSceneClosing += () =>
            {
                OnUpdateSongReceived = (_, _) => { };
                OnSendPrepared = _ => { };
                OnBackReceived = _ => { };
                OnGetRoomInfoSucceeded = _ => { };
                OnConnecting = () => { };
                OnConnectSucceeded = () => { };
                OnConnectFailed = () => { };
                OnDisconnect = () => { };
                OnLoginSucceeded = () => { };
                OnCreateRoomSucceeded = () => { };
                OnCloseRoomSucceeded = () => { };
                OnJoinRoomSucceeded = () => { };
                OnQuitRoomSucceeded = () => { };
                OnUpdateSongSucceeded = () => { };
                OnStartGameSucceeded = () => { };
                OnGameStarted = () => { };
                OnSendMessageSucceeded = () => { };
                OnGetRoomSongIdSucceeded = () => { };
            };
        }

        public static void Set(bool online, bool chart)
        {
            isOnline = online;
            enableChart = chart;
        }

        public static bool EnableChartUpload => enableChart;

        public static int CreateSocket(string serverUrl)
        {
            Close();
            // if (socket.Connected) return -1; // 已连接
            if (!General.TryParseHost(serverUrl, out IPEndPoint endPoint, out SocketManager.serverUrl))
                return -2; // 地址不合法
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

        public static int CreateSocket1(string serverUrl)
        {
            Close();
            // if (socket.Connected) return -1; // 已连接
            ManualResetEvent TimeoutObject = new ManualResetEvent(false);
            if (!General.TryParseHost(serverUrl, out IPEndPoint endPoint, out SocketManager.serverUrl))
                return -2; // 地址不合法
            try
            {
                socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                OnConnecting.Invoke();
                socket.BeginConnect(endPoint, EndConnect, socket);
                // socket.Connect(endPoint);
                if (TimeoutObject.WaitOne(5000, false))
                {
                    OnConnectSucceeded.Invoke();
                    StartReceive();
                    return 0;
                }

                OnConnectFailed.Invoke();
                return -3; // 无法连接
            }
            catch (SocketException e)
            {
                e.Print();
                OnConnectFailed.Invoke();
                return -3; // 无法连接
            }

            void EndConnect(IAsyncResult result)
            {
                TimeoutObject.Set();
                socket.EndConnect(result);
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
                    Username = LoginManager.Username,
                    VerifyToken = LoginManager.VerifyToken
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

        public static int SyncRoom()
        {
            if (roomId == "") return -4;
            return GeneralSend(ClientOperate.Room_Sync, GetSendDataWithToken());
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

        public static int UpdateSong(string songId, SongType type)
        {
            SendDataWithToken pack = GetSendDataWithToken();
            pack.Addition.Add("songId", songId);
            pack.Addition.Add("songType", type.ToString());
            return GeneralSend(ClientOperate.Room_UpdateSong, pack);
        }

        public static int Ready()
        {
            if (isReady) return -4;
            return GeneralSend(ClientOperate.User_Ready, GetSendDataWithToken());
        }

        public static int Unready()
        {
            if (!isReady) return -4;
            return GeneralSend(ClientOperate.User_UnReady, GetSendDataWithToken());
        }

        public static int EndGame(string score, string acc)
        {
            SendDataWithToken pack = GetSendDataWithToken();
            pack.Addition.Add("score", score);
            pack.Addition.Add("acc", acc);
            // 因为sky脑抽所以要改动
            // ClientOperate clientOperate = ClientOperate.Room_UserGameEnd;
            // GeneralSendData value = pack;
            // lock (threadLock)
            // {
            //     if (!socket.Connected) return -1; // 未连接
            //     if (string.IsNullOrEmpty(token)) return -3; // 未登录
            //     try
            //     {
            //         currentClientOperate = clientOperate;
            //         OnSendPrepared.Invoke(currentClientOperate);
            //         value.Operate = clientOperate.ToString();
            //         string messageToSend = JsonConvert.SerializeObject(value);
            //         JObject fuckSky = JObject.Parse(messageToSend);
            //         fuckSky.Add("score", pack.Addition["score"]); // 写到外层
            //         fuckSky.Add("acc", pack.Addition["acc"]); // 写到外层
            //         messageToSend = fuckSky.ToString();
            //         Debug.Log("尝试发送" + messageToSend);
            //         socket.Send(new UTF8Encoding(false).GetBytes(messageToSend));
            //         return 0;
            //     }
            //     catch (SocketException e)
            //     {
            //         e.Print();
            //         return -2; // 无法发送
            //     }
            // }
            return GeneralSend(ClientOperate.User_GameEnd, pack);
        }

        public static int QuitGame()
        {
            return GeneralSend(ClientOperate.Room_UserQuitGame, GetSendDataWithToken());
        }

        #endregion

        public static void GetSong()
        {
            OnUpdateSongReceived.Invoke(songId, songType);
        }

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
                Username = LoginManager.Username,
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
                        AnalyzePack(pack, clientOperate);
                    }
                }
            });
            return 0;
        }

        private static async void AnalyzePack(JObject pack, ClientOperate clientOperate)
        {
            // await new WaitWhile(() => InGameUIManager.IsActive);
            if (!pack.ContainsKey("Type"))
            {
                Debug.Log("错误：无法处理接收到的数据");
                Debug.Log(pack); 
                return;
            }

            await UniTask.SwitchToMainThread();
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
                            roomId = createRoomReceive.RoomId;
                            isOwner = true;
                            OnCreateRoomSucceeded.Invoke();
                        }, printOnSuccess: true);
                    break;
                case ClientOperate.User_CloseRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法关闭房间", succeededCallback: _ => { isReady = false; });
                    break;
                case ClientOperate.User_JoinRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法加入房间", succeededCallback: _ =>
                    {
                        roomId = tryJoinRoomId;
                        isOwner = false;
                        OnJoinRoomSucceeded.Invoke();
                    }, printOnSuccess: true);
                    break;
                case ClientOperate.Room_UpdateSong:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法更换谱面",
                        succeededCallback: _ => OnUpdateSongSucceeded.Invoke());
                    break;
                case ClientOperate.Room_GameStart:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法开始游戏",
                        succeededCallback: _ =>
                        {
                            allGameEnded = false;
                            OnStartGameSucceeded.Invoke();
                        });
                    break;
                case ClientOperate.Room_SendMessage:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法发送信息",
                        succeededCallback: _ => OnSendMessageSucceeded.Invoke());
                    break;
                case ClientOperate.Room_GetRoomSongId:
                    DealWithMsg<GetSongIdReceive>(pack, "错误：无法获取歌曲id",
                        succeededCallback: _ => OnGetRoomSongIdSucceeded.Invoke());
                    break;
                case ClientOperate.Room_Sync:
                    DealWithMsg<RoomInfoReceive>(pack, "错误：无法获取房间信息",
                        succeededCallback: receive =>
                        {
                            songId = receive.RoomInfo.SelectedSongID;
                            songType = Enum.Parse<SongType>(receive.RoomInfo.SelectedSongType, true);
                            OnGetRoomInfoSucceeded.Invoke(receive.RoomInfo);
                        },
                        failedCallback: LocalQuitRoom);
                    break;
                case ClientOperate.User_QuitRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法退出房间", succeededCallback: _ => LocalQuitRoom(),
                        printOnSuccess: true);
                    break;
                case ClientOperate.User_Ready:
                    isReady = true;
                    break;
                case ClientOperate.User_UnReady:
                    isReady = false;
                    break;
                case ClientOperate.User_GameEnd:
                    break;
                case ClientOperate.Room_UserQuitGame:
                    break;
                case ClientOperate.User_LeaveServer:
                default:
                    throw new ArgumentOutOfRangeException(nameof(clientOperate), clientOperate, null);
            }
        }

        private static void LocalQuitRoom()
        {
            ChatManager.AddMessage("Server", "已退出房间" + roomId, MessageType.Server);
            roomId = "";
            isReady = false;
            isOwner = false;
            OnQuitRoomSucceeded.Invoke();
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
                serverId = received.serverId;
                OnLoginSucceeded.Invoke();
                InGameUIManager.ShowModalWindowWithClose("提示", received.Message, () => { }, "确认");
                ChatManager.AddMessage("Server", $"已加入服务器{serverUrl}", MessageType.Server);
            }
        }

        private static void DealWithMsg<T>(JObject jObject,
            string errorMessage, [CanBeNull] Func<T, bool> checkData = null,
            [CanBeNull] Action<T> succeededCallback = null,
            bool printOnSuccess = false, [CanBeNull] Action failedCallback = null)
            where T : BackReceiveData
        {
            T? received = jObject.ToObject<T>();
            if (received == null) throw new ArgumentException("Unknown Exception");
            Debug.Log("成功接收到数据：" + JsonConvert.SerializeObject(received, Formatting.None));
            string serializeObject = JsonConvert.SerializeObject(received);
            if (!received.Status)
            {
                if (failedCallback == null)
                {
                    ChatManager.AddMessage("Server", errorMessage + "——" + received.Message, MessageType.Error);
                    Debug.Log("错误：无法完成操作\n" + serializeObject);
                }
                else failedCallback.Invoke();
            }
            else
            {
                if (checkData != null && !checkData.Invoke(received))
                {
                    ChatManager.AddMessage("Server", errorMessage + "\n返回包体有误\n" + serializeObject, MessageType.Error);
                    Debug.Log("返回的数据有误：\n" + received.Message);
                    return;
                }

                succeededCallback?.Invoke(received);
                if (printOnSuccess) ChatManager.AddMessage("Server", received.Message, MessageType.Server);
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

            switch (serverOperate)
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
                                ChatManager.AddMessage(receive.Author, receive.Message, MessageType.Server);
                                break;
                            case "User_Ready":
                            case "User_UnReady":
                                ChatManager.AddMessage(receive.Author, receive.Message, MessageType.Room);
                                break;
                            case "User_GameEnd":
                                ChatManager.AddMessage(receive.Author, receive.Message, MessageType.Room);
                                break;
                            case "Room_AllGameEnd":
                                allGameEnded = true;
                                ChatManager.AddMessage(receive.Author, receive.Message, MessageType.Room);
                                break;
                            default:
                                ChatManager.AddMessage(receive.Author, receive.Message, MessageType.Server);
                                Debug.Log("错误：暂且未知的Server NewMessage operate种类：" + receive.Author);
                                break;
                        }
                    }
                    else
                    {
                        ChatManager.AddMessage(receive.Author, receive.Message,
                            receive.Author == LoginManager.Username ? MessageType.Self : MessageType.Common);
                    }

                    break;
                case ServerOperate.GameStart:
                    ChatManager.AddMessage("Server", "游戏开始", MessageType.Room);
                    OnGameStarted.Invoke();
                    break;
                case ServerOperate.RoomClosed:
                    ChatManager.AddMessage("Server", "房间已关闭", MessageType.Server);
                    (isOwner ? OnCloseRoomSucceeded : OnQuitRoomSucceeded).Invoke();
                    roomId = "";
                    isReady = false;
                    isOwner = false;
                    break;
                case ServerOperate.UpdateSong:
                    Debug.Log("试图更新歌曲信息：" +
                              JsonConvert.SerializeObject(pack.ToObject<UpdaeSongActiveReceive>(), Formatting.None));
                    songId = pack["songId"].ToString();
                    songType = Enum.Parse<SongType>(pack["songType"].ToString(), true);
                    OnUpdateSongReceived.Invoke(songId, songType);
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

        public static string GetServerId()
        {
            return serverId;
        }
        
        public static bool CanStartGame => isOwner && allGameEnded;

        public static bool IsOwner => isOwner;

        public static void Disconnect() => Close();

        private static void Close()
        {
            if (socket == null) return;
            stopReceive = true;
            taskReceive = null;
            if (roomId != "") (isOwner ? OnCloseRoomSucceeded : OnQuitRoomSucceeded).Invoke();
            try
            {
                socket.Disconnect(false);
                socket.Close();
            }
            catch (SocketException)
            {
            }

            OnDisconnect.Invoke();
            socket = null;
            token = serverId = roomId = tryJoinRoomId = "";
            isOwner = isReady = false;
            allGameEnded = true;
            songId = "";
            songType = SongType.empty;
        }
    }
}