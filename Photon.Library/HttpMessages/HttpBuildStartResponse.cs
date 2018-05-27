using Newtonsoft.Json;

namespace Photon.Library.HttpMessages
{
    public class HttpBuildStartResponse
    {
        [JsonProperty("sessionId")]
        public string SessionId {get; set;}

        [JsonProperty("buildNumber")]
        public uint BuildNumber {get; set;}
    }
}
