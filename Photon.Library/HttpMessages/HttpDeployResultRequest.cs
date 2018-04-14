using Newtonsoft.Json;

namespace Photon.Library.HttpMessages
{
    public class HttpDeployResultRequest
    {
        [JsonProperty("session")]
        public string ServerSessionId {get; set;}
    }
}
