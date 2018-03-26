using Newtonsoft.Json;

namespace Photon.Library.Models
{
    public class SessionBeginRequest
    {
        [JsonProperty("projectName")]
        public string ProjectName {get; set;}

        [JsonProperty("releaseVersion")]
        public string ReleaseVersion {get; set;}
    }
}
