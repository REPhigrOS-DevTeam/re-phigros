using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MainCore;
using MainCore.Common;
using MainCore.Data;
using MainCore.UI;
using MainCore.Utilities;
using Network.Chart;
using Network.Multiplayer.Data;
using Network.Multiplayer.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

public class MPServerTest : MonoBehaviour
{
    public ChatManager chatManager;

    public Button bDisconnect;
    public Button bCreateRoom, bSelectRoom, bCloseRoom, bQuitRoom, bDownloadSong, bStartGame, bUpdateSong;
    public Toggle_Button bReady;
    public Toggle isPublicOnCreateRoom;

    private static SongType selectedSongType = SongType.empty;
    private static string selectedSongId = "";
    private static SongInfo selectedSongInfo = null;
    private static bool downloaded = false;

    public Animator roomListAnimator;
    public Button bRefreshRoomList;
    public RectTransform rtRoomList;
    public GameObject roomListItemPrefab;

    private int roomId = -1;

    public Text tConnectState;

    public Text tLoginToken, tRoomId;

    public GameObject loginObj, createRoomObj;

    private static int? unityThreadId;
    private bool IsFromUnityThread => unityThreadId == null || unityThreadId == Thread.CurrentThread.ManagedThreadId;

    public GameObject sendMask, downloadMask, uploadMask;

    private RoomSummary[] roomList = null;

    private Dictionary<GameObject, RoomState> buttonToState = new();

    private static string ownerLocalPath = "";

    private static string[] generalErrorMessages = { "未连接服务器", "无法发送数据包", "你小子没登录" };

    private void Awake()
    {
        unityThreadId = Thread.CurrentThread.ManagedThreadId;
        // ZipConstants.DefaultCodePage = 65001; // UTF-8
        buttonToState = new()
        {
            { bCreateRoom.gameObject, RoomState.NotInRoom },
            { bCloseRoom.gameObject, RoomState.RoomOwner },
            { bQuitRoom.gameObject, RoomState.RoomMember },
            { bDownloadSong.gameObject, RoomState.RoomMember | RoomState.RoomOwner },
            { bStartGame.gameObject, RoomState.RoomOwner },
            { bUpdateSong.gameObject, RoomState.RoomOwner },
            { bReady.gameObject, RoomState.RoomMember }
        };
        SocketManager.OnCloseRoomSucceeded += ChartHandler.OnRoomClosed;
        SocketManager.OnQuitRoomSucceeded += ChartHandler.OnRoomQuited;
        // SocketManager.OnCloseRoomSucceeded += chatManager.OnInitOrRoomClosed;
        // SocketManager.OnQuitRoomSucceeded += chatManager.OnInitOrRoomClosed;
        // SocketManager.OnCreateRoomSucceeded += chatManager.OnRoomJoinedOrCreated;
        // SocketManager.OnJoinRoomSucceeded += chatManager.OnRoomJoinedOrCreated;
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
        createRoomObj.SetActive(false);
        tConnectState.text = "服务器状态：未连接";
        bDisconnect.onClick.AddListener(Disconnect);

        bSelectRoom.onClick.AddListener(() => {roomListAnimator.SetTrigger(Enabled);});
        roomListAnimator.gameObject.GetComponent<Button>().onClick.AddListener(() => {roomListAnimator.SetTrigger(Disabled);});
        bRefreshRoomList.onClick.AddListener(() =>
        {
            bRefreshRoomList.interactable = false;
            GeneralListener(SocketManager.FetchRoomList, generalErrorMessages);
            bRefreshRoomList.interactable = true;
        });
        bCreateRoom.onClick.AddListener(CreateRoomPrepare);
        bCloseRoom.onClick.AddListener(() => GeneralListener(SocketManager.CloseRoom, generalErrorMessages));
        bQuitRoom.onClick.AddListener(() => GeneralListener(SocketManager.QuitRoom, generalErrorMessages));
        bReady.onOnLabel = "取消准备";
        bReady.onOffLabel = "准备";
        bReady.OnValueChanged += OnReadyButtonValueChanged;
        bStartGame.onClick.AddListener(() => GeneralListener(SocketManager.StartGame, generalErrorMessages));
        bUpdateSong.onClick.AddListener(() =>
        {
            async Task OnGetFileSuccess(string[] paths)
            {
                int state = SocketManager.UpdateSong(await ChartHandler.Upload(ownerLocalPath = paths[0]),
                    SongType.rep, (await GameUtils.GetSongInfo(paths[0])).Item1);
                if (state == 0) return;
                ChatManager.AddMessage("Server", generalErrorMessages[-state - 1], MessageType.Error);
            }

            uploadMask.SetActive(true);
            FileBrowser.ShowLoadDialog(async paths =>
                {
                    await OnGetFileSuccess(paths);
                    uploadMask.SetActive(false);
                }, () => { uploadMask.SetActive(false); }, FileBrowser.PickMode.Folders, false,
                PlayerPrefs.GetString("file_path", Application.persistentDataPath), null, "选择谱面...", "上传");
        });
        bDownloadSong.onClick.AddListener(Download);
        SocketManager.OnLoginSucceeded += () => { loginObj.SetActive(false); };
        SocketManager.OnCreateRoomSucceeded += () =>
        {
            SetButtonState(RoomState.RoomOwner);
            SetDownloaded(false);
        };
        SocketManager.OnCloseRoomSucceeded += () =>
        {
            SetButtonState(RoomState.NotInRoom);
            selectedSongId = "";
            selectedSongType = SongType.empty;
        };
        SocketManager.OnJoinRoomSucceeded += () =>
        {
            SetButtonState(RoomState.RoomMember);
            SocketManager.FetchRoomInfo();
        };
        SocketManager.OnGetRoomInfoSucceeded += info =>
        {
            OnUpdateSongReceived(info.SelectedSongID, Enum.Parse<SongType>(info.SelectedSongType),
                info.selectedSongInfo);
        };
        SocketManager.OnQuitRoomSucceeded += () =>
        {
            SetButtonState(RoomState.NotInRoom);
            selectedSongId = "";
            selectedSongType = SongType.empty;
            selectedSongInfo = null;
        };
        SocketManager.OnSendPrepared += clientOperate =>
        {
            if (clientOperate is ClientOperate.Room_SendMessage or ClientOperate.User_LoginToServer
                or ClientOperate.Room_UpdateSong) return;
            sendMask.SetActive(true);
        };
        SocketManager.OnBackReceived += clientOperate => { sendMask.SetActive(false); };
        SocketManager.OnUpdateSongReceived += OnSongReceived;
        SocketManager.OnGameStarted += EnterGame;
        SocketManager.OnGetRoomListSucceeded += list =>
        {
            roomList = list;
            for (int i = 0; i < rtRoomList.childCount; i++)
            {
                Destroy(rtRoomList.GetChild(i).gameObject);
            }
            foreach (RoomSummary summary in roomList)
            {
                GameObject o = Instantiate(roomListItemPrefab);
                o.GetComponent<RoomListItem>().Set(this, summary);
            }
        };
        sendMask.SetActive(false);
        if (SocketManager.GetToken() != "")
        {
            tConnectState.text = "服务器状态：连接成功";
            loginObj.SetActive(false);
            SetButtonState(SocketManager.GetRoomId() == "" ? RoomState.NotInRoom :
                SocketManager.IsOwner ? RoomState.RoomOwner : RoomState.RoomMember);
            SetDownloaded(downloaded && SocketManager.GetSongId() == selectedSongId &&
                          SocketManager.GetSongType() == selectedSongType);
            // if (SocketManager.GetRoomId() != "") SocketManager.GetSong();
        }
        else
        {
            SetButtonState(RoomState.NotInRoom);
        }

        GlobalSetting.Reset();
    }

    private void SelectRoom()
    {
        roomListAnimator.SetTrigger(Enabled);
        if (roomList == null) bRefreshRoomList.onClick.Invoke();
    }

    public void SelectRoom(int id)
    {
        if (SocketManager.GetRoomId() != "") return;
        chatManager.SetText(id.ToString());
        roomListAnimator.SetTrigger(Disabled);
    }

    private int createRoomFlag;

    private async void CreateRoomPrepare()
    {
        // () => GeneralListener(SocketManager.CreateRoom, generalErrorMessages)
        createRoomObj.SetActive(true);
        createRoomFlag = 0;
        await UniTask.WaitWhile(() => createRoomFlag == 0);
        createRoomObj.SetActive(false);
        if (createRoomFlag < 0) return;
        GeneralListener(() => SocketManager.CreateRoom(isPublicOnCreateRoom.isOn), generalErrorMessages);
    }

    public void SetCreateRoomFlag(int flag)
    {
        createRoomFlag = flag;
    }

    private void OnSongReceived(string id, SongType type, SongInfo info)
    {
        ChatManager.AddMessage("", SocketManager.IsOwner ? "您已更新曲目" : "房主更新了曲目", MessageType.Room);
        if (type == SongType.rep)
            ChatManager.AddMessage("",
                "曲目信息：\n" +
                $" - ID：{id}\n" +
                $" - 歌曲名称：{info.SongName}\n" +
                $" - 作曲家：{info.SongComposer}\n" +
                $" - 难度：{info.SongDifficulty}\n" +
                $" - 谱师：{info.SongCharter}\n" +
                $" - 曲绘绘师：{info.SongIllustrator}\n" +
                $" - 曲目长度：{info.MusicLength:0.##}s\n" +
                $" - [备用]文件夹名称：{info.FolderName}",
                MessageType.Room);
        OnUpdateSongReceived(id, type, JObject.FromObject(info));
        if (!SocketManager.IsOwner) return;
        bDownloadSong.interactable = false;
        OwnerOperation();

        async void OwnerOperation()
        {
            if (await MoveSong())
            {
                ChatManager.AddMessage("downloadSucceeded", $"成功上传谱面，id：" + id, MessageType.Server);
                SetDownloaded(true);
            }
            else
            {
                ChatManager.AddMessage("downloadFailed", $"错误：未知错误", MessageType.Error);
                SetDownloaded(false);
            }
        }
    }

    private async void Download()
    {
        if (selectedSongType == SongType.empty) return;
        downloadMask.SetActive(true);
        if (await DownloadSong())
        {
            ChatManager.AddMessage("downloadSucceeded",
                $"成功从{selectedSongType switch { SongType.rep => "官方谱面服务器", SongType.Phizone => "PhiZone谱面服务器", SongType.empty => throw new ArgumentOutOfRangeException(), _ => throw new ArgumentOutOfRangeException() }}下载谱面{selectedSongId}",
                MessageType.Server);
            SetDownloaded(true);
        }
        else
        {
            ChatManager.AddMessage("downloadFailed",
                $"错误：无法从{selectedSongType switch { SongType.rep => "官方谱面服务器", SongType.Phizone => "PhiZone谱面服务器", SongType.empty => throw new ArgumentOutOfRangeException(), _ => throw new ArgumentOutOfRangeException() }}下载谱面{selectedSongId}，或谱面文件不规范",
                MessageType.Error);
            SetDownloaded(false);
        }

        downloadMask.SetActive(false);
    }

    private void OnReadyButtonValueChanged(bool isOn)
    {
        if (isOn)
            SocketManager.Ready();
        else
            SocketManager.Unready();
    }

    private void Disconnect()
    {
        selectedSongId = "";
        selectedSongType = SongType.empty;
        SocketManager.Disconnect();
        SceneTransit.Instance.JumpScene("NetworkTest");
    }

    public void UpdateConnectState(string str)
    {
        tConnectState.text = $"服务器状态：{str}";
    }

    private void OnUpdateSongReceived(string id, SongType type, JObject songInfo)
    {
        selectedSongId = id;
        selectedSongType = type;
        if (type == SongType.rep)
        {
            selectedSongInfo = songInfo.ToObject<SongInfo>();
        }
        else if (type == SongType.Phizone)
        {
            // TODO: 接入PhiZone
        }

        SetDownloaded(false);
        bReady.IsOn = false;
    }

    private void GeneralListener(Func<int> getState, params string[] errorMessages)
    {
        int state = getState.Invoke();
        if (state == 0) return;
        ChatManager.AddMessage("Server", errorMessages[-state - 1], MessageType.Error);
    }

    private PhiraInfoData phiraInfoData;
    private static readonly int Enabled = Animator.StringToHash("Enabled");
    private static readonly int Disabled = Animator.StringToHash("Disabled");

    private string GetDirectory()
    {
        string directory = $"{ChartHandler.TmpPathRoot}/decompressed_online_charts/rep/{selectedSongId}";
        Debug.Log("Directory: " + directory);
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
        Directory.CreateDirectory(directory);
        return directory;
    }

    private async Task<bool> MoveSong()
    {
        string directory = GetDirectory();
        CopyFolder(ownerLocalPath, directory);
        return await ProcessChart(directory);
    }

    private async Task<bool> DownloadSong() // TODO: 接入PhiZone
    {
        string directory = GetDirectory();

        ZipUtils.Unzip(await ChartHandler.Download(selectedSongId), directory);

        // 如果出现了zip里是单个根文件夹的情况就把文件夹里的东西移出来
        string[] entries = Directory.GetFileSystemEntries(directory);
        if (entries.Length == 1 && Directory.Exists(entries[0]))
        {
            string[] directories = Directory.GetDirectories(entries[0]);
            foreach (var qwq in directories)
            {
                Directory.Move(qwq, directory);
            }

            string[] files = Directory.GetFiles(entries[0]);
            foreach (var awa in files)
            {
                File.Move(awa, directory);
            }

            Directory.Delete(entries[0]);
        }

        return await ProcessChart(directory);
    }

    private async Task<bool> ProcessChart(string directory)
    {
#if true
        GlobalSetting.chartFolderPath = directory;
        string originalPath = PlayerPrefs.GetString("chartFolderPath", Application.persistentDataPath);
        PlayerPrefs.SetString("chartFolderPath", directory);
        PlayerPrefs.Save();

        SongInfo songInfo;
        GameFilePathInfo pathInfo;
        object obj;
        (songInfo, GlobalSetting.infoType, pathInfo, obj) = await GameUtils.GetInfoForPlay(directory);

        switch (GlobalSetting.infoType)
        {
            case InfoType.Empty:
                GlobalSetting.chartName = "Unknown";
                GlobalSetting.difficulty = "SP  Lv.?";
                GlobalSetting.charter = "Unknown";
                GlobalSetting.composer = "Unknown";
                GlobalSetting.illustrator = "Unknown";

                try
                {
                    GlobalSetting.chartPath = Path.Combine(directory,
                        GameUtils.SelectGivenExtensionsFileNames(directory, ".json", ".pec")[0]);
                    GlobalSetting.musicPath = Path.Combine(directory,
                        GameUtils.SelectGivenExtensionsFileNames(directory, ".wav", ".ogg", ".mp3")[0]);
                    GlobalSetting.illustrationPath = Path.Combine(directory,
                        GameUtils.SelectGivenExtensionsFileNames(directory, ".png", ".bmp", ".jpg", ".jpeg")[0]);
                }
                catch (ArgumentOutOfRangeException)
                {
                    PlayerPrefs.SetString("chartFolderPath", originalPath);
                    PlayerPrefs.Save();
                    ChatManager.AddMessage("Server", "错误：谱面包缺失某些文件", MessageType.Error);
                    return false;
                }

                break;
            case InfoType.InfoTxt:
            case InfoType.InfoCsv:
            case InfoType.InfoCsvOld:
            case InfoType.InfoYml:
                GlobalSetting.chartName = songInfo.SongName;
                GlobalSetting.difficulty = songInfo.SongDifficulty;
                GlobalSetting.charter = songInfo.SongCharter;
                GlobalSetting.composer = songInfo.SongComposer;
                GlobalSetting.illustrator = songInfo.SongIllustrator;
                GlobalSetting.chartPath = Path.Combine(directory, pathInfo.Chart);
                GlobalSetting.musicPath = Path.Combine(directory, pathInfo.Music);
                GlobalSetting.illustrationPath = Path.Combine(directory, pathInfo.Illustration);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await Main.InitChartAuto(GlobalSetting.chartPath, false, false).ConfigureAwait(false);
        if (GlobalSetting.infoType == InfoType.InfoYml) Main.ApplyPhiraOffset((float)obj);

        // extra init
        string extraJsonPath = directory + "/extra.json";
        if (File.Exists(extraJsonPath))
        {
            try
            {
                GlobalSetting.extraEvents =
                    JsonConvert.DeserializeObject<Extra>(await File.ReadAllTextAsync(extraJsonPath));
            }
            catch (Exception)
            {
                GlobalSetting.extraEvents = null;
            }
        }
        else
        {
            GlobalSetting.extraEvents = null;
        }

        if (GlobalSetting.extraEvents is { Videos: { Count: > 0 } })
        {
            InGameUIManager.ShowModalWindowWithClose("警告", "RPGR不支持视频\n除非你愿意捐400美金", () => { }, "确定");
            await UniTask.WaitWhile(() => InGameUIManager.IsActive);
        }

        // chart init
        GlobalSetting.lineImage = File.Exists(directory + "/line.csv") ? new CSVReader(directory + "/line.csv") : null;
        // convert illustration & music
        await UniTask.SwitchToMainThread();
        GlobalSetting.backgroundImage =
            Util.ReadFileAsSprite(await File.ReadAllBytesAsync(GlobalSetting.illustrationPath), out Exception e);
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

    private void EnterGame()
    {
        // other
        GlobalSetting.isMultiplayer = true;
        GlobalSetting.YayaKawaii = GlobalSetting.YayaMode.冲;
        GlobalSetting.PepoyoDaisuki = GlobalSetting.PepoyoMode.Waraninja;
        GlobalSetting.usingApi = false;
        GlobalSetting.ReadUserSettings();
        GlobalSetting.recordMode = false;
        GlobalSetting.autoPlay = false;
        GlobalSetting.useCourseMode = false;
        GlobalSetting.Pitch = 1.0f;
        HitSoundManager.Init();
        PopupMessageManager.Instance.Clear();
        SceneTransit.Instance.LoadScene("LoadInto");
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
        if (!IsFromUnityThread || Convert.ToString((int)state, 2).Replace("0", "").Length > 1) return;
        foreach (GameObject button in buttonToState.Keys)
        {
            button.SetActive((buttonToState[button] & state) == state);
        }

        if (state == RoomState.NotInRoom)
        {
            chatManager.OnInitOrRoomClosed();
            SetDownloaded(false);
        }
        else
        {
            chatManager.OnRoomJoinedOrCreated();
        }
    }

    private void SetDownloaded(bool value)
    {
        if (!IsFromUnityThread)
        {
            throw new ArgumentException("Not from Unity thread");
        }

        downloaded = value;
        bDownloadSong.interactable = !value && selectedSongType != SongType.empty;
        bReady.Interactable = value;
        bStartGame.interactable = value && SocketManager.CanStartGame;
    }

    public void Back()
    {
        SceneTransit.Instance.Back();
    }

    /// <summary>
    /// 复制文件夹及文件
    /// </summary>
    /// <param name="sourceFolder">原文件路径</param>
    /// <param name="destFolder">目标文件路径</param>
    /// <returns></returns>
    public bool CopyFolder(string sourceFolder, string destFolder)
    {
        try
        {
            if (!Directory.Exists(sourceFolder)) return false;
            //如果目标路径不存在,则创建目标路径
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            //得到原文件根目录下的所有文件
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest); //复制文件
            }

            //得到原文件根目录下的所有文件夹
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest); //构建目标路径,递归复制文件
            }

            return true;
        }
        catch (Exception e)
        {
            e.Print();
            return false;
        }
    }
}

[Flags]
public enum RoomState
{
    NotInRoom = 1 << 0,
    RoomOwner = 1 << 1,
    RoomMember = 1 << 2
}