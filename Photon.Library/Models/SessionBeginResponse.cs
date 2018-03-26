using Newtonsoft.Json;

namespace Photon.Library.Models
{
    public class SessionBeginResponse
    {
        [JsonProperty("sessionId")]
        public string SessionId {get; set;}
    }
}
