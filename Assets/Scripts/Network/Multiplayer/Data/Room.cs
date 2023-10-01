using Newtonsoft.Json;

namespace Network.Multiplayer.Data
{
    public class CreateRoomReceive : BackReceiveData
    {
        [JsonProperty("RoomID")] public string? RoomId;
    }

    public class GetSongIdReceive : BackReceiveData
    {
        [JsonProperty("songId")] public RoomInfo SongId;
    }

    public class MessageActiveReceive : ActiveReceiveData
    {
        [JsonProperty("from")] public string Author;
        [JsonProperty("isServer")] public bool IsServer;
        [JsonProperty("message")] public string Message;
    }

    public class UpdaeSongActiveReceive : ActiveReceiveData
    {
        [JsonProperty("songId")] public string songId;
        [JsonProperty("songInfo")] public SongInfo songInfo;
    }

    public class RoomInfoReceive : BackReceiveData
    {
        [JsonProperty("SyncReturn")] public RoomInfo RoomInfo;
    }

    public class RoomInfo
    {
        [JsonProperty("Room_PlayerList")] public string[] PlayerList;
        [JsonProperty("Room_SongType")] public string SelectedSongType;
        [JsonProperty("Room_SongId")] public string SelectedSongID;
        [JsonProperty("Room_SongInfo")] public SongInfo selectedSongInfo;
    }

    public enum MessageType
    {
        Common = 0,
        Self = 1,
        Server = 2,
        Error = 3,
        Debug = 4,
        Room = 5
    }
}
