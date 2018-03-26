using Newtonsoft.Json;

namespace Photon.Library.Models
{
    public class SessionReleaseRequest
    {
        [JsonProperty("sessionId")]
        public string SessionId {get; set;}
    }
}
