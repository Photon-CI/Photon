using Newtonsoft.Json;

namespace Photon.Library
{
    public class ProjectSourceDefinition
    {
        [JsonProperty("type")]
        public string Type {get; set;}

        [JsonProperty("source")]
        public string Source {get; set;}
    }
}
