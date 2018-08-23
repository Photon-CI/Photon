using Newtonsoft.Json;

namespace Photon.Library.Http.Messages
{
    public class HttpBuildResultRequest
    {
        [JsonProperty("session")]
        public string ServerSessionId {get; set;}
    }
}
