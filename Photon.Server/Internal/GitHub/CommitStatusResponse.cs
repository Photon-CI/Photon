using Newtonsoft.Json;

namespace Photon.Server.Internal.GitHub
{
    public class CommitStatusResponse
    {
        [JsonProperty("id")]
        public int Id {get; set;}

        [JsonProperty("url")]
        public string Url {get; set;}
    }
}
