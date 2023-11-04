using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using MainCore;
using MainCore.Common;
using MainCore.Utilities;
using Network.Multiplayer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
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
        private static SongInfo songInfo = null;
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

        public static Action<string, SongType, SongInfo> OnUpdateSongReceived = (_, _, _) => { };

        public static Action<ClientOperate>
            OnSendPrepared = _ => { },
            OnBackReceived = _ => { };

        public static Action<RoomInfo> OnGetRoomInfoSucceeded = _ => { };

        public static Action<RoomSummary[]> OnGetRoomListSucceeded = _ => { };
        public static Action
            OnConnecting = () => { },
            OnConnectSucceeded = () => { },
            OnConnectFailed = () => { },
            OnDisconnect = () => { },
            OnLoginSucceeded = () => { },
            OnLoginFailed = () => { },
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
                OnUpdateSongReceived = (_, _, _) => { };
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
                    Username = GlobalSetting.username,
                    VerifyToken = GlobalSetting.verifyToken
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

        public static int FetchRoomList()
        {
            if (!socket.Connected) return -1; // 未连接
            if (token == "") return -3; // 未登录
            return GeneralSend(ClientOperate.Server_Sync);
        }

        public static int CreateRoom(bool isPublic)
        {
            return GeneralSend(ClientOperate.User_CreateNewRoom, ("IsPublic", isPublic));
        }

        public static int CloseRoom()
        {
            return GeneralSend(ClientOperate.User_CloseRoom);
        }

        public static int JoinRoom(string roomId)
        {
            if (!socket.Connected) return -1; // 未连接
            if (token == "") return -3; // 未登录
            tryJoinRoomId = roomId;
            return GeneralSend(ClientOperate.User_JoinRoom, ("RoomID", roomId));
        }

        public static int FetchRoomInfo()
        {
            if (roomId == "") return -4;
            return GeneralSend(ClientOperate.Room_Sync);
        }

        public static int QuitRoom()
        {
            return GeneralSend(ClientOperate.User_QuitRoom);
        }

        public static int SendRoomMessage(string msg)
        {
            return GeneralSend(ClientOperate.Room_SendMessage, ("Message", msg));
        }

        public static int StartGame()
        {
            return GeneralSend(ClientOperate.Room_GameStart);
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

        public static int UpdateSong(string songId, SongType type, SongInfo songInfo = null)
        {
            if (type == SongType.empty) return -4; // ????
            return GeneralSend(ClientOperate.Room_UpdateSong, ("songId", songId), ("songType", type.ToString()), ("songInfo",
                type == SongType.rep ? songInfo : null));
        }

        public static int Ready()
        {
            if (isReady) return -4;
            return GeneralSend(ClientOperate.User_Ready);
        }

        public static int Unready()
        {
            if (!isReady) return -4;
            return GeneralSend(ClientOperate.User_UnReady);
        }

        public static int EndGame(string score, string acc)
        {
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
            return GeneralSend(ClientOperate.User_GameEnd, ("score", score), ("acc", acc));
        }

        public static int QuitGame()
        {
            return GeneralSend(ClientOperate.Room_UserQuitGame);
        }

        #endregion
        
        private static int GeneralSend(ClientOperate clientOperate, params (string, object)[] additions)
        {
            lock (threadLock)
            {
                if (!socket.Connected) return -1; // 未连接
                if (token == "") return -3; // 未登录
                var pack = GetSendDataWithToken();
                foreach ((string, object) tuple in additions)
                {
                    pack.Addition.Add(tuple.Item1, tuple.Item2);                    
                }
                try
                {
                    currentClientOperate = clientOperate;
                    OnSendPrepared.Invoke(currentClientOperate);
                    pack.Operate = clientOperate.ToString();
                    socket.Send(pack);
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
                Username = GlobalSetting.username,
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
                case ClientOperate.Server_Sync:
                    DealWithMsg<SyncRoomReceive>(pack, "错误：无法获取房间列表",
                       succeededCallback: syncRoomReceive =>
                        {
                            OnGetRoomListSucceeded.Invoke(syncRoomReceive.List);
                        });
                    break;
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
                            if (songType == SongType.rep) songInfo = receive.RoomInfo.selectedSongInfo.ToObject<SongInfo>();
                            else if (songType == SongType.Phizone)
                            {
                                // TODO: 接入PhiZone
                            }

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
                OnLoginFailed.Invoke();
            }
            else
            {
                if (received.token == null)
                {
                    InGameUIManager.ShowModalWindowWithClose("错误", errorMessage + "\n返回包体有误\n" + serializeObject,
                        () => { }, "确认");
                    Debug.Log("返回的数据有误：\n" + received.Message);
                    OnLoginFailed.Invoke();
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
                    ChatManager.AddMessage("Server", errorMessage + (received.Message == null ? "" : "——" + received.Message), MessageType.Error);
                    Debug.Log("错误：无法完成操作\n" + serializeObject);
                }
                else failedCallback.Invoke();
            }
            else
            {
                if (checkData != null && !checkData.Invoke(received))
                {
                    ChatManager.AddMessage("Server", errorMessage + "\n返回包体有误\n" + serializeObject, MessageType.Error);
                    if (received.Message != null) Debug.Log("返回的数据有误：\n" + received.Message);
                    return;
                }

                succeededCallback?.Invoke(received);
                if (printOnSuccess && received.Message != null) ChatManager.AddMessage("Server", received.Message, MessageType.Server);
            }
        }

        private static void ExecuteActivePack(JObject pack)
        {
            Debug.Log("收到Active包：" + JsonConvert.SerializeObject(pack));
            if (!pack.ContainsKey("operate"))
            {
                Debug.Log("错误：非法Active包\n" + JsonConvert.SerializeObject(pack));
                return;
            }

            if (!Enum.TryParse(pack["operate"].ToString(), out ServerOperate serverOperate))
            {
                Debug.Log("错误：未知的服务器操作\n" + JsonConvert.SerializeObject(pack));
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
                            case "User_QuitGame":
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
                            receive.Author == GlobalSetting.username ? MessageType.Self : MessageType.Common);
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
                    UpdateSongActiveReceive updateSongActiveReceive = pack.ToObject<UpdateSongActiveReceive>();
                    Debug.Log("试图更新歌曲信息：" + JsonConvert.SerializeObject(updateSongActiveReceive, Formatting.None));
                    songId = updateSongActiveReceive.songId;
                    songType = Enum.Parse<SongType>(updateSongActiveReceive.songType, true);
                    if (songType == SongType.rep) songInfo = updateSongActiveReceive.songInfo;
                    OnUpdateSongReceived.Invoke(songId, songType, songInfo);
                    break;
                case ServerOperate.ServerClosed:
                    Util.QuitApp();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetSongId()
        {
            return songId;
        }

        public static SongType GetSongType()
        {
            return songType;
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

        public static void Disconnect()
        {
            LeaveServer();
        }

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
            songInfo = null;
        }
    }
}