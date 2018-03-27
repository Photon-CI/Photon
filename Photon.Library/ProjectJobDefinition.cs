using Newtonsoft.Json;

namespace Photon.Library
{
    public class ProjectJobDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("assembly")]
        public string Assembly {get; set;}

        [JsonProperty("script")]
        public string Script {get; set;}

        [JsonProperty("preBuild")]
        public string PreBuild {get; set;}

        [JsonProperty("postBuild")]
        public string PostBuild {get; set;}
    }
}
