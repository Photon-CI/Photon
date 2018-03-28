using Newtonsoft.Json;

namespace Photon.Framework.Sessions
{
    public class SessionReleaseRequest
    {
        [JsonProperty("sessionId")]
        public string SessionId {get; set;}
    }
}
