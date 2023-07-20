using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Network.Multiplayer;
using Network.Verify.API;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class MPServerTest : MonoBehaviour
{
    public InputField ifUsername, ifUrl;
    public InputField ifRoomId;
    public InputField ifMessage;

    public Button bConnect, bDisconnect, bLogin;
    public Button bCreateRoom, bJoinRoom, bCloseRoom, bNewMessage;

    private static readonly Regex usernameRegex = new Regex("[^a-zA-Z0-9]");

    public static string Username = "Sky";

    private int roomId = -1;

    public Text tConnectState;

    public Text tLoginToken, tRoomId;

    // Start is called before the first frame update
    void Start()
    {
        // Username = RepAPI.Username;
        tConnectState.text = "服务器状态：未连接";
        bConnect.onClick.AddListener(() =>
        {
            tConnectState.text = "服务器状态：连接中";
            int state = SocketUtil.CreateSocket(ifUrl.text);
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
                SocketUtil.Close();
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

        string[] generalErrorMessages = { "未连接服务器", "无法发送信息", "你小子没登录" };
        bLogin.onClick.AddListener(() => GeneralListener(() => SocketUtil.Login(Username), generalErrorMessages));
        bCreateRoom.onClick.AddListener(() => GeneralListener(SocketUtil.CreateRoom, generalErrorMessages));
        bJoinRoom.onClick.AddListener(() =>
        {
            if (roomId < 0)
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "未输入房间id", () => { }, "确定");
                return;
            }

            GeneralListener(() => SocketUtil.JoinRoom(roomId + ""), generalErrorMessages);
        });
        bCloseRoom.onClick.AddListener(() => GeneralListener(SocketUtil.CloseRoom, generalErrorMessages));
        bNewMessage.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(ifMessage.text))
            {
                InGameUIManager.ShowModalWindowWithClose("错误", "信息为空", () => { }, "确定");
                return;
            }
            GeneralListener(() => SocketUtil.SendRoomMessage(ifMessage.text), generalErrorMessages);
        });
        ifUsername.onEndEdit.AddListener(CheckUsername);
        ifUsername.text = Username;
        ifMessage.onEndEdit.AddListener(CheckMessage);
        ifRoomId.onEndEdit.AddListener(CheckRoomId);
        ifRoomId.text = "";
    }

    private void GeneralListener(Func<int> getState, params string[] errorMessages)
    {
        int state = getState.Invoke();
        if (state == 0) return;
        InGameUIManager.ShowModalWindowWithClose("错误", errorMessages[-state - 1], () => { }, "确定");
    }

    private void Update()
    {
        string token = SocketUtil.GetToken();
        string roomId = SocketUtil.GetRoomId();
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

    private void CheckRoomId(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            roomId = -1;
        }
        else if (int.TryParse(input, out int i))
        {
            roomId = i;
        }
        else
        {
            ifRoomId.text = roomId < 0 ? "" : roomId + "";
        }
    }

    private void CheckMessage(string input)
    {
        ifMessage.text = input.TrimEnd();
    }
}