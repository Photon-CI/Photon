using Newtonsoft.Json;

namespace Photon.Framework.Sessions
{
    public class SessionReleaseResponse
    {
        [JsonProperty("successful")]
        public bool Successful {get; set;}
    }
}
