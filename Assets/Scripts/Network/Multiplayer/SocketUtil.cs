using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading.Tasks;
using Baracuda.Threading;
using JetBrains.Annotations;
using Network.Multiplayer.Data;
using Network.Verify.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Network.Multiplayer
{
    public class SocketUtil : MonoBehaviour
    {
        private static Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static string username = "";
        private static string token = "";
        private static string roomId = "";
        private static string tryJoinRoomId = "";
        private static Task receiveTask = null;
        private static JObject[] packs;
        private static bool stopReceive = false;
        public static ClientOperate CurrentClientOperate = ClientOperate.LoginToServer;

        public static int CreateSocket(string serverUrl)
        {
            socket ??= new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (socket.Connected) return -1; // 已连接
            var strings = serverUrl.Split(":");
            if (strings.Length != 2 || !int.TryParse(strings[1], out int port)) return -2; // 地址不合法
            try
            {
                socket.Connect(strings[0], port);
                StartReceive();
                return 0;
            }
            catch (SocketException e)
            {
                e.Print();
                return -3; // 无法连接
            }
        }

        public static int Login(string username)
        {
            if (!socket.Connected) return -1; // 未连接
            try
            {
                SocketUtil.username = username;
                CurrentClientOperate = ClientOperate.LoginToServer;
                LoginSendData<string> pack = new LoginSendData<string>
                {
                    Operate = CurrentClientOperate.ToString(),
                    Username = MPServerTest.Username,
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
                GetSendDataWithToken<string>(ClientOperate.User_CreateNewRoom));
        }

        public static int JoinRoom(string roomId)
        {
            tryJoinRoomId = roomId;
            SendDataWithToken<string> pack = GetSendDataWithToken<string>(ClientOperate.User_JoinRoom);
            pack.Addition.Add("RoomID", roomId);
            return GeneralSend(ClientOperate.User_JoinRoom, pack);
        }

        public static int CloseRoom()
        {
            return GeneralSend(ClientOperate.User_CloseRoom,
                GetSendDataWithToken<string>(ClientOperate.User_CloseRoom));
        }

        public static int SendRoomMessage(string msg)
        {
            SendDataWithToken<string> pack = GetSendDataWithToken<string>(ClientOperate.Room_SendMessage);
            pack.Addition.Add("NewMessage", msg);
            return GeneralSend(ClientOperate.Room_SendMessage, pack);
        }

        private static int GeneralSend<T>(ClientOperate clientOperate, GeneralSendData<T> value)
        {
            if (!socket.Connected) return -1; // 未连接
            if (string.IsNullOrEmpty(token)) return -3; // 未登录
            try
            {
                CurrentClientOperate = clientOperate;
                socket.Send(value);
                return 0;
            }
            catch (SocketException e)
            {
                e.Print();
                return -2; // 无法发送
            }
        }

        private static SendDataWithToken<T> GetSendDataWithToken<T>(ClientOperate operate)
        {
            SendDataWithToken<T> pack = new SendDataWithToken<T>
            {
                Operate = operate.ToString(),
                Username = MPServerTest.Username,
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
                    ClientOperate clientOperate = CurrentClientOperate;
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
            switch (clientOperate)
            {
                case ClientOperate.LoginToServer:
                    DealWithMsg<LoginReceive>(pack, "错误：无法登录", loginReceive => loginReceive.token != null,
                        loginReceive => token = loginReceive.token);
                    break;
                case ClientOperate.User_CreateNewRoom:
                    DealWithMsg<CreateRoomReceive>(pack, "错误：无法创建房间",
                        createRoomReceive => createRoomReceive.RoomId != null,
                        createRoomReceive => roomId = createRoomReceive.RoomId);
                    break;
                case ClientOperate.User_CloseRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法关闭房间", callback: _ => roomId = "");
                    break;
                case ClientOperate.User_JoinRoom:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法加入房间", callback: _ => roomId = tryJoinRoomId);
                    break;
                case ClientOperate.Room_UpdateSong:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法更换谱面");
                    break;
                case ClientOperate.Room_GameStart:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法开始游戏");
                    break;
                case ClientOperate.Room_SendMessage:
                    DealWithMsg<BackReceiveData>(pack, "错误：无法发送信息");
                    break;
                case ClientOperate.Room_GetRoomSongId:
                    DealWithMsg<GetSongIdReceive>(pack, "错误：无法获取歌曲id");
                    break;
                case ClientOperate.Room_GetRoomInfo:
                    DealWithMsg<RoomInfoReceive>(pack, "错误：无法获取房间信息");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clientOperate), clientOperate, null);
            }
        }

        private static void DealWithMsg<T>(JObject jObject,
            string errorMessage, [CanBeNull] Func<T, bool> checkData = null, [CanBeNull] Action<T> callback = null)
            where T : BackReceiveData
        {
            T? received = jObject.ToObject<T>();
            if (received == null) throw new ArgumentException("Unknown Exception");
            Debug.Log("成功接收到数据：" + JsonConvert.SerializeObject(received, Formatting.None));
            if (!received.Status)
            {
                Debug.Log("错误：无法完成操作\n" + JsonConvert.SerializeObject(received));
            }
            else
            {
                if (checkData != null && !checkData.Invoke(received))
                {
                    Debug.Log(errorMessage + "\n" + received.Message);
                    return;
                }

                callback?.Invoke(received);
            }

            InGameUIManager.ShowModalWindowWithClose(received.Status ? "信息" : "错误", received.Message, () => { }, "确定");
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
                    break;
                case ServerOperate.GameStart:
                    break;
                case ServerOperate.RoomClosed:
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

        public static void Close()
        {
            if (socket == null) return;
            stopReceive = false;
            receiveTask = null;
            try
            {
                socket.Disconnect(false);
            }
            catch (SocketException)
            {
            }

            socket.Close();
            socket = null;
        }
    }
}