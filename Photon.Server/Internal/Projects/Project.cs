using Newtonsoft.Json;

namespace Photon.Server.Internal.Projects
{
    internal class Project
    {
        [JsonProperty("id")]
        public string Id {get; set;}

        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("description")]
        public string Description {get; set;}
    }
}
