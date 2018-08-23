using Newtonsoft.Json;

namespace Photon.Library.Http.Messages
{
    public class HttpDeployStartResponse
    {
        [JsonProperty("sessionId")]
        public string SessionId {get; set;}
    }
}
