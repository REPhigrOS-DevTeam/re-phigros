using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Network.Multiplayer.Components;
using Network.Multiplayer.Managers;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using MessageType = Network.Multiplayer.Data.MessageType;

public class MPServerTest : MonoBehaviour
{
    public ChatManager chatManager;
    public InputField ifUsername, ifUrl;

    public Button bConnect, bDisconnect, bLogin;
    public Button bCreateRoom, bCloseRoom;

    private static readonly Regex usernameRegex = new Regex("[^a-zA-Z0-9]");

    public static string Username = "Sky";

    private int roomId = -1;

    public Text tConnectState;

    public Text tLoginToken, tRoomId;

    public GameObject loginObj;

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
                SocketManager.Close();
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
        bLogin.onClick.AddListener(() => GeneralListener(() => SocketManager.Login(Username), generalErrorMessages));
        bCreateRoom.onClick.AddListener(() => GeneralListener(SocketManager.CreateRoom, generalErrorMessages));
        bCloseRoom.onClick.AddListener(() => GeneralListener(SocketManager.CloseRoom, generalErrorMessages));
        ifUsername.onEndEdit.AddListener(CheckUsername);
        ifUsername.text = Username;
        SocketManager.Init(chatManager);
        SocketManager.OnLoginSucceeded += () => { loginObj.SetActive(false); };
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

    private void CheckUsername(string input)
    {
        if (usernameRegex.IsMatch(input) || string.IsNullOrEmpty(input))
        {
            ifUsername.text = Username;
        }
        else
        {
            Username = input;
            // RepAPI.Username = Username;
        }
    }
}