using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using MainCore;
using MainCore.Common;
using MainCore.Data;
using MainCore.UI;
using MainCore.Utilities;
using Network.Account;
using Network.API;
#if UNITY_EDITOR
using Network.Chart;
#endif
using Network.Multiplayer.Components;
using Network.Multiplayer.Data;
using Network.Multiplayer.Managers;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using MessageType = Network.Multiplayer.Data.MessageType;

public class MPServerTest : MonoBehaviour
{
    public ChatManager chatManager;
    public InputField ifUrl;

    public Button bConnect, bDisconnect, bLogin;
    public Button bCreateRoom, bCloseRoom,bQuitRoom, bDownloadSong, bStartGame, bUpdateSong;
    public Toggle_Button bReady;

    private string selectedSongId = "";

    private int roomId = -1;

    public Text tConnectState;

    public Text tLoginToken, tRoomId;

    public GameObject loginObj;
    
    private static int? unityThreadId = null;
    private bool IsFromUnityThread => unityThreadId == null || unityThreadId == Thread.CurrentThread.ManagedThreadId;

    public GameObject sendMask;

    private Dictionary<Button, RoomState> buttonToState = new();

    private void Awake()
    {
        unityThreadId = Thread.CurrentThread.ManagedThreadId;
        // ZipConstants.DefaultCodePage = 65001; // UTF-8
        buttonToState = new()
        {
            { bCreateRoom, RoomState.NotInRoom },
            { bCloseRoom, RoomState.RoomOwner },
            { bQuitRoom, RoomState.RoomMember },
            { bDownloadSong, RoomState.RoomMember | RoomState.RoomOwner },
            { bStartGame, RoomState.RoomOwner },
            { bUpdateSong, RoomState.RoomOwner }
        };
        SocketManager.OnCloseRoomSucceeded += ChartHandler.OnRoomClosed;
        SocketManager.OnQuitRoomSucceeded += ChartHandler.OnRoomQuited;
        GlobalSetting.ReadUserSettings();
        chatManager.RevertChatHistory();
        // try
        // {
        //     InitAPI();
        // }
        // catch (ArgumentException)
        // {
        // }
    }

    // private async void InitAPI()
    // {
    //     await UniTask.SwitchToMainThread();
    //     PopupMessageManager.Instance.Message("尝试连接api服务器……");
    //     LoginManager.ReadAccountFromPlayerPrefs();
    //     bool succeeded = await RepAPI.Init();
    //     if (succeeded)
    //     {
    //         PopupMessageManager.Instance.Message("连接成功");
    //     }
    //     else
    //     {
    //         InGameUIManager.ShowModalWindowWithClose("致命错误", "无法连接至服务器\n程序即将退出", Util.QuitApp, "确定");
    //     }
    // }

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
        bDisconnect.onClick.AddListener(Util.QuitApp);

        string[] generalErrorMessages = { "未连接服务器", "无法发送数据包", "你小子没登录" };
        bLogin.onClick.AddListener(() =>
            GeneralListener(SocketManager.Login, generalErrorMessages[0], generalErrorMessages[1], "已经登录"));
        bCreateRoom.onClick.AddListener(() => GeneralListener(SocketManager.CreateRoom, generalErrorMessages));
        bCloseRoom.onClick.AddListener(() => GeneralListener(SocketManager.CloseRoom, generalErrorMessages));
        bQuitRoom.onClick.AddListener(() => GeneralListener(SocketManager.QuitRoom, generalErrorMessages));
        ifUrl.onEndEdit.AddListener(str =>
        {
            if (str.Trim() == ifUrl.text) return;
            ifUrl.text = str.Trim();
        });
        bReady.OnValueChanged += (button, text, isOn) =>
        {
            text.text = isOn ? "取消准备" : "准备";
            if (isOn) SocketManager.Ready();
            else SocketManager.Unready();
        };
        bStartGame.onClick.AddListener(() => GeneralListener(SocketManager.StartGame, generalErrorMessages));
        bUpdateSong.onClick.AddListener(() => GeneralListener(() => SocketManager.UpdateSong("63ae61e1272f"), generalErrorMessages));
        bDownloadSong.onClick.AddListener(() =>
        {
            OnUpdateSongReceived("63ae61e1272f");
        });
        SocketManager.Init(chatManager);
        SocketManager.OnLoginSucceeded += () => { loginObj.SetActive(false); };
        SocketManager.OnCreateRoomSucceeded += () =>
        {
            SetButtonState(RoomState.RoomOwner);
            bDownloadSong.interactable = false;
        };
        SocketManager.OnCloseRoomSucceeded += () => { SetButtonState(RoomState.NotInRoom); };
        SocketManager.OnJoinRoomSucceeded += () => { SetButtonState(RoomState.RoomMember); };
        SocketManager.OnQuitRoomSucceeded += () => { SetButtonState(RoomState.NotInRoom);};
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
        SocketManager.OnUpdateSongReceived += OnUpdateSongReceived;
        SocketManager.OnGameStarted += EnterGame;
        SocketManager.OnJoinRoomSucceeded += () => SocketManager.SyncRoom();
        SetButtonState(RoomState.NotInRoom);
        sendMask.SetActive(false);
        bReady.IsOn = false;
#if UNITY_EDITOR
        // Test.Qwq();
#endif
    }

    private async void OnUpdateSongReceived(string id)
    {
        sendMask.SetActive(true);
        SetDownloaded(false);
        bReady.IsOn = false;
        if (await DownloadSong(id))
        {
            selectedSongId = id;
            ChatManager.AddMessage("downloadSucceeded", "成功下载谱面" + id, MessageType.Server);
            SetDownloaded(true);
        }
        else
        {
            ChatManager.AddMessage("downloadFailed", "错误：无法下载谱面" + id, MessageType.Error);
            SetDownloaded(false);
        }
        sendMask.SetActive(false);
    }

    private void GeneralListener(Func<int> getState, params string[] errorMessages)
    {
        int state = getState.Invoke();
        if (state == 0) return;
        ChatManager.AddMessage("Server", errorMessages[-state - 1], MessageType.Error);
    }

    private PhiraInfoData phiraInfoData = null;

    private async Task<bool> DownloadSong(string id) // TODO: 接入PhiZone
    {
#if true
        string directory = Application.dataPath + "/../Debug/Songs/" + id;
        if (!Directory.Exists(Application.dataPath + "/../Debug/Songs/" + id))
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "未知的歌曲id：" + id, () => { }, "确定");
            return false;
        }

        string debugInfoFile = directory + "/debug.json";
        if (!File.Exists(debugInfoFile))
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "歌曲信息不存在", () => { }, "确定");
            return false;
        }

        DebugChartInfo debugChartInfo =
            JsonConvert.DeserializeObject<DebugChartInfo>(await File.ReadAllTextAsync(debugInfoFile));
        if (debugChartInfo == null)
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "歌曲信格式有误", () => { }, "确定");
            return false;
        }
        // path init
        GlobalSetting.chartPath = directory + "/" + debugChartInfo.chartFileName;
        GlobalSetting.musicPath = directory + "/" + debugChartInfo.musicFileName;
        GlobalSetting.illustrationPath = directory + "/" + debugChartInfo.illustraionFileName;
        PlayerPrefs.SetString("chartFolderPath", directory);
        PlayerPrefs.Save();
        // phira info
        string phiraInfoPath = directory + "/info.yml";
        phiraInfoData = null;
        if (File.Exists(phiraInfoPath))
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            phiraInfoData = deserializer.Deserialize<PhiraInfoData>(await File.ReadAllTextAsync(phiraInfoPath));
            GlobalSetting.difficulty = phiraInfoData.level;
        }
        // info init
        if (phiraInfoData == null)
        {
            var infoPath = directory + "/info.txt";
            if (File.Exists(infoPath))
            {
                GlobalSetting.infoTxt = new InfoTxtReader(infoPath);
                GlobalSetting.chartName = GlobalSetting.infoTxt.GetName();
                GlobalSetting.difficulty = GlobalSetting.infoTxt.GetDifficulty();
            }
            else
            {
                GlobalSetting.infoTxt = null;
                GlobalSetting.chartName = debugChartInfo.name;
                GlobalSetting.difficulty = debugChartInfo.difficulte + " Lv." + Mathf.FloorToInt(debugChartInfo.hard);
            }
        }

        GlobalSetting.charter = debugChartInfo.charter;
        GlobalSetting.composer = debugChartInfo.composer;
        GlobalSetting.illustrator = debugChartInfo.illustrator;
        // extra init
        string extraJsonPath = directory + "/extra.json";
        if (File.Exists(extraJsonPath))
        {
            GlobalSetting.extraJson = await File.ReadAllTextAsync(extraJsonPath);
        }
        // chart init
        GlobalSetting.lineImage = File.Exists(directory + "/line.csv") ? new CSVReader(directory + "/line.csv") : null;
        GlobalSetting.chartFolderPath = directory;
        await Main.InitChartAuto(phiraInfoData != null && !string.IsNullOrEmpty(phiraInfoData.chart) ? GlobalSetting.chartFolderPath + "/" + phiraInfoData.chart : GlobalSetting.chartPath, false).ConfigureAwait(false);
        Main.OverloadInfoWithPhiraYaml(phiraInfoData);
        // convert illustration & music
        await UniTask.SwitchToMainThread();
        GlobalSetting.backgroundImage = Util.ReadFileAsSprite(await File.ReadAllBytesAsync(GlobalSetting.illustrationPath), out Exception e);
        if (e != null)
        {
            Debug.LogError(e);
            InGameUIManager.ShowModalWindowWithClose("错误", "无法读取曲绘", () => { }, "确定");
        }
        Main.music = await Util.ReadMusicAsAudioClip(GlobalSetting.musicPath);
#else
        ChartInfo chartInfo = ChartInfo.FromJson(await File.ReadAllTextAsync(debugInfoFile));
        if (chartInfo.Chart == null)
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "没有谱面", () => { }, "确定");
            return;
        }

        if (chartInfo.Song.Song == null)
        {
            InGameUIManager.ShowModalWindowWithClose("错误", "没有歌曲", () => { }, "确定");
            return;
        }

        byte[] chartData = await chartInfo.Chart.SendGetRequestAsync();
        byte[] musicData = await chartInfo.Song.Song.SongSong.SendGetRequestAsync();
        byte[] illustrationData = await chartInfo.Song.Song.Illustration.SendGetRequestAsync();
#endif
        return true;
    }

    private void EnterGame() {
        // other
        GlobalSetting.YayaKawaii = GlobalSetting.YayaMode.冲;
        GlobalSetting.PepoyoDaisuki = GlobalSetting.PepoyoMode.Waraninja;
        GlobalSetting.usingApi = false;
        GlobalSetting.ReadUserSettings();
        GlobalSetting.recordMode = false;
        GlobalSetting.autoPlay = false;
        HitSoundManager.Init();
        PopupMessageManager.Instance.Clear();
        SceneTransit.Instance.TransitTo("LoadInto");
    }

    // private int counter = 0;

    private void Update()
    {
        string token = SocketManager.GetToken();
        string roomId = SocketManager.GetRoomId();
        tLoginToken.text = "Login Token: " + (string.IsNullOrEmpty(token) ? "未登录" : token);
        tRoomId.text = "Room Id: " + (string.IsNullOrEmpty(roomId) ? "无" : roomId);
        // if (!Input.GetMouseButtonDown(2)) return;
        // switch (counter % 2)
        // {
        //     case 0:
        //         Debug.Log("开始压缩");
        //         DateTime dateTime = DateTime.Now;
        //         ZipUtils.ZipDirectory("G:/RPGR-Data/でんでん心電図", "G:/RPGR-Data", "test");
        //         DateTime dateTime1 = DateTime.Now;
        //         Debug.Log("压缩完了,用时：" + (dateTime1 - dateTime).TotalMilliseconds + "ms");
        //         break;
        //     case 1:
        //         Debug.Log("开始解压");
        //         dateTime = DateTime.Now;
        //         ZipUtils.UnZip("G:/RPGR-Data/test.zip", "G:/RPGR-Data/test");
        //         dateTime1 = DateTime.Now;
        //         Debug.Log("解压完了,用时：" + (dateTime1 - dateTime).TotalMilliseconds + "ms");
        //         break;
        // }
        // counter++;
    }

    private void SetButtonState(RoomState state)
    {
        if (!IsFromUnityThread || Convert.ToString((int) state, 2).Replace("0", "").Length > 1) return;
        foreach (Button button in buttonToState.Keys)
        {
            button.gameObject.SetActive((buttonToState[button] & state) == state);
        }

        if (state == RoomState.NotInRoom)
        {
            SetDownloaded(false);
        }
    }

    private void SetDownloaded(bool value)
    {
        bDownloadSong.interactable = !value && selectedSongId != "";
        bReady.Interactable = value;
        bStartGame.interactable = value;
    }
}

public class DebugChartInfo
{
    public string chartFileName = "";
    public string musicFileName = "";
    public string illustraionFileName = "";
    public string name = "";
    public string composer = "";
    public string charter = "";
    public string illustrator = "";
    public string difficulte = ""; // 难度标识
    public float hard = 0.0f;
}

[Flags]
public enum RoomState
{
    NotInRoom = 1 << 0,
    RoomOwner = 1 << 1,
    RoomMember = 1 << 2
}