using Newtonsoft.Json;

namespace Photon.Library.Http.Messages
{
    public class HttpSessionStartResponse
    {
        [JsonProperty("sessionId")]
        public string SessionId {get; set;}
    }
}
