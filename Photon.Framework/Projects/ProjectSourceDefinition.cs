using Newtonsoft.Json;

namespace Photon.Framework.Projects
{
    public class ProjectSourceDefinition
    {
        [JsonProperty("type")]
        public string Type {get; set;}

        [JsonProperty("source")]
        public string Source {get; set;}
    }
}
