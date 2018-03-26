using Newtonsoft.Json;

namespace Photon.Library.Models
{
    public class SessionReleaseResponse
    {
        [JsonProperty("successful")]
        public bool Successful {get; set;}
    }
}
