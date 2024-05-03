using Newtonsoft.Json;

namespace Network.Account.Serialized
{
    public class Manifest
    {
        [JsonProperty("name")] public string name { get; set; }
        [JsonProperty("english_name")] public string english_name { get; set; }
        [JsonProperty("space_english_name")] public string space_english_name { get; set; }
        [JsonProperty("domain")] public string domain { get; set; }
        [JsonProperty("protocol")] public string protocol { get; set; }
        [JsonProperty("iconurl")] public string iconurl { get; set; }
        [JsonProperty("apiOnline")] public ApiOnline apiOnline { get; set; }
        [JsonProperty("apiURL")] public ApiURL apiURL { get; set; }
    }

    public class ApiOnline
    {
        [JsonProperty("chart")] public bool chart { get; set; }
    }

    public class ApiURL
    {
        [JsonProperty("user-login")] public string userlogin { get; set; }

        [JsonProperty("user-verify")] public string userverify { get; set; }
        [JsonProperty("chart-domain")] public string chartHost { get; set; }
        [JsonProperty("chart-port")] public int chartPort { get; set; }
    }
    
//     {
//     "name": "REPhigrOS Team",
//     "english_name": "REPhigrOSTeam",
//     "space_english_name": "REPhigrOS Team",
//     "domain": "rephigros.top",
//     "protocol": "https",
//     "agreement": "https",
//     "iconurl": "cdn.rephigros.top/imgs/web.png",
//     "apiURL": {
//     "user-login": "/auth/login",
//     "user-verify": "/auth/verify",
//     "chart-ip": "114.514.191.98"
//     }
}