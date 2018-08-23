using Newtonsoft.Json;

namespace Photon.Library.Http.Messages
{
    public class HttpDeployResultRequest
    {
        [JsonProperty("session")]
        public string ServerSessionId {get; set;}
    }
}
