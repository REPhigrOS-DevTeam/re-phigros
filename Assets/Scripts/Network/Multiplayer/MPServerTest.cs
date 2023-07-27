using System;
using System.Net.Sockets;
using System.Threading;
using Baracuda.Threading;
using MainCore.UI;
using MainCore.Utilities;
using Network.Multiplayer.Components;
using Network.Multiplayer.Data;
using Network.Multiplayer.Managers;
using Network.Verify.API;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using MessageType = Network.Multiplayer.Data.MessageType;

public class MPServerTest : MonoBehaviour
{
    public ChatManager chatManager;
    public InputField ifUrl;

    public Button bConnect, bDisconnect, bLogin;
    public Button bCreateRoom, bCloseRoom;
    public Button bStartGame;

    private int roomId = -1;

    public Text tConnectState;

    public Text tLoginToken, tRoomId;

    public GameObject loginObj;

    public GameObject goCreateRoomButtons, goRoomOwnerButtons, goRoomMemberButtons;

    private static int unityThreadId;
    private Func<bool> IsFromUnityThread = () => true;

    public GameObject sendMask;

    private void Awake()
    {
        unityThreadId = Thread.CurrentThread.ManagedThreadId;
        IsFromUnityThread = () => unityThreadId == Thread.CurrentThread.ManagedThreadId;
        try
        {
            InitAPI();
        }
        catch (ArgumentException)
        {
        }
    }

    private async void InitAPI()
    {
        await Dispatcher.InvokeAsync(() => PopupMessageManager.Instance.Message("尝试连接api服务器……"));
        bool succeeded = await RepAPI.Init();
        if (succeeded)
        {
            await Dispatcher.InvokeAsync(() => PopupMessageManager.Instance.Message("连接成功"));
        }
        else
        {
            await Dispatcher.InvokeAsync(() =>
                InGameUIManager.ShowModalWindowWithClose("致命错误", "无法连接至服务器\n程序即将退出", Util.QuitApp, "确定"));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Username = RepAPI.Username;
        loginObj.SetActive(true);
        tConnectState.text = "服务器状态：未连接";
        bConnect.onClick.AddListener(() =>
        {
            tConnectState.text = "服务器状态：连接中";
            int state = SocketManager.CreateSocket(ifUrl.text);
            if (state == 0)
            {
                tConnectState.text = "服务器状态：连接成功";
                return;
            }

            tConnectState.text = "服务器状态：连接失败";
            InGameUIManager.ShowModalWindowWithClose("错误", state switch
            {
                -1 => "已经连接服务器",
                -2 => "服务器Url不合法",
                -3 => "无法连接服务器，可能是未启动",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown Exception")
            }, () => { }, "确定");
        });
        bDisconnect.onClick.AddListener(() =>
        {
            tConnectState.text = "服务器状态：断开连接中";
            try
            {
                SocketManager.LeaveServer();
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            catch (SocketException e)
            {
                tConnectState.text = "服务器状态：断开连接失败";
                InGameUIManager.ShowModalWindowWithClose("错误", "无法断开连接：" + e.Message + "\n" + e.StackTrace, () => { },
                    "确定");
            }
        });

        string[] generalErrorMessages = { "未连接服务器", "无法发送数据包", "你小子没登录" };
        bLogin.onClick.AddListener(() =>
            GeneralListener(SocketManager.Login, generalErrorMessages[0], generalErrorMessages[1], "已经登录"));
        bCreateRoom.onClick.AddListener(() => GeneralListener(SocketManager.CreateRoom, generalErrorMessages));
        bCloseRoom.onClick.AddListener(() => GeneralListener(SocketManager.CloseRoom, generalErrorMessages));
        ifUrl.onEndEdit.AddListener(str =>
        {
            if (str.Trim() == ifUrl.text) return;
            ifUrl.text = str.Trim();
        });
        bStartGame.onClick.AddListener(() => GeneralListener(SocketManager.StartGame, generalErrorMessages));
        SocketManager.Init(chatManager);
        SocketManager.OnLoginSucceeded += () => { loginObj.SetActive(false); };
        SocketManager.OnCreateRoomSucceeded += () => { SetButtonState(RoomState.RoomOwner); };
        SocketManager.OnCloseRoomSucceeded += () => { SetButtonState(RoomState.NotInRoom); };
        SocketManager.OnJoinRoomSucceeded += () => { SetButtonState(RoomState.RoomMember); };
        SocketManager.OnSendPrepared += clientOperate =>
        {
            if (clientOperate == ClientOperate.Room_SendMessage) return;
            sendMask.SetActive(true);
        };
        SocketManager.OnBackReceived += clientOperate =>
        {
            if (clientOperate == ClientOperate.Room_SendMessage) return;
            sendMask.SetActive(false);
        };
        SetButtonState(0);
        sendMask.SetActive(false);
    }

    private void GeneralListener(Func<int> getState, params string[] errorMessages)
    {
        int state = getState.Invoke();
        if (state == 0) return;
        chatManager.AddMessage("Server", errorMessages[-state - 1], MessageType.Error);
    }

    private void Update()
    {
        string token = SocketManager.GetToken();
        string roomId = SocketManager.GetRoomId();
        tLoginToken.text = "Login Token: " + (string.IsNullOrEmpty(token) ? "未登录" : token);
        tRoomId.text = "Room Id: " + (string.IsNullOrEmpty(roomId) ? "无" : roomId);
    }

    private void SetButtonState(RoomState state)
    {
        if (!IsFromUnityThread.Invoke()) return;
        goCreateRoomButtons.SetActive(state == RoomState.NotInRoom);
        goRoomOwnerButtons.SetActive(state == RoomState.RoomOwner);
        goRoomMemberButtons.SetActive(state == RoomState.RoomMember);
    }
}

public enum RoomState
{
    NotInRoom = 0,
    RoomOwner = 1,
    RoomMember = 2
}