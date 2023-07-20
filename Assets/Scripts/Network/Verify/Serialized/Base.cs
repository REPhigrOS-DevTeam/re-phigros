using Newtonsoft.Json;

namespace Network.Verify.Serialized
{
    public class Base
    {
        public string status { get; set; }
        public string AuthMode { get; set; }

        [JsonProperty("Auth-Application-Author")]
        public string AuthApplicationAuthor { get; set; }

        [JsonProperty("Auth-Application-Owner")]
        public string AuthApplicationOwner { get; set; }

        [JsonProperty("Auth-Application-Description")]
        public string AuthApplicationDescription { get; set; }

        public bool TestMode { get; set; }
        public string AuthVersionMode { get; set; }
        public string AuthVersion { get; set; }
    }
}