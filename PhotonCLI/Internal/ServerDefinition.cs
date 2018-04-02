using Newtonsoft.Json;

namespace Photon.CLI.Internal
{
    public class ServerDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("url")]
        public string Url {get; set;}

        [JsonProperty("primary")]
        public bool Primary {get; set;}
    }
}
