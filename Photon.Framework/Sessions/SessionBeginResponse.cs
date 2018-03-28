using Newtonsoft.Json;

namespace Photon.Framework.Sessions
{
    public class SessionBeginResponse
    {
        [JsonProperty("sessionId")]
        public string SessionId {get; set;}
    }
}
