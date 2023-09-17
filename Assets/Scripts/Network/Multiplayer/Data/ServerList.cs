using System.Collections.Generic;
using Newtonsoft.Json;

namespace Network.Multiplayer.Data
{
    public class ServerList
    {
        [JsonProperty("Servers")] public List<Server> servers = new();
    }

    public class Server
    {
        [JsonProperty("Custom-Name")] public string customName;
        [JsonProperty("Url")] public string url;
    }
}