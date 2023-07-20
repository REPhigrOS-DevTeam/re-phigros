using Newtonsoft.Json;

namespace Network.Verify.Serialized
{
    public class Manifest
    {
        public string name { get; set; }
        public string english_name { get; set; }
        public string space_english_name { get; set; }
        public string domain { get; set; }
        public string protocol { get; set; }
        public string agreement { get; set; }
        public string iconurl { get; set; }
        public ApiURL apiURL { get; set; }
    }

    public class ApiURL
    {
        [JsonProperty("user-login")] public string userlogin { get; set; }

        [JsonProperty("user-verify")] public string userverify { get; set; }
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
//     "user-login": "api.rephigros.top/auth/login",
//     "user-verify": "api.rephigros.top/auth/verify"
// }
// }
}