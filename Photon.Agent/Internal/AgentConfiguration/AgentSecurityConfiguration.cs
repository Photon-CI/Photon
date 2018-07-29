using Newtonsoft.Json;

namespace Photon.Agent.Internal.AgentConfiguration
{
    public class AgentSecurityConfiguration
    {
        [JsonProperty("enabled")]
        public bool Enabled {get; set;}

        [JsonProperty("type")]
        public string Type {get; set;}
    }
}
