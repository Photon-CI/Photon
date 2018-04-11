using Newtonsoft.Json;

namespace Photon.Library.HttpMessages
{
    public class HttpBuildResultRequest
    {
        [JsonProperty("session")]
        public string ServerSessionId {get; set;}
    }
}
