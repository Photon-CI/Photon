using Newtonsoft.Json;

namespace Photon.Framework.Sessions
{
    public class SessionBeginRequest
    {
        [JsonProperty("projectName")]
        public string ProjectName {get; set;}

        [JsonProperty("releaseVersion")]
        public string ReleaseVersion {get; set;}
    }
}
