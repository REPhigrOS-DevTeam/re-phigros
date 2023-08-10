using Newtonsoft.Json;

namespace Network.Multiplayer.Data
{
    public class LoginReceive : BackReceiveData
    {
        [JsonProperty("token")]
        public string? token;
        [JsonProperty("ServerID")] public string? serverId;
    }
}