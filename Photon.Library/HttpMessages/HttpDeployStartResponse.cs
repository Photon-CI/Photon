using Newtonsoft.Json;

namespace Photon.Library.HttpMessages
{
    public class HttpDeployStartResponse
    {
        [JsonProperty("sessionId")]
        public string SessionId {get; set;}
    }
}
