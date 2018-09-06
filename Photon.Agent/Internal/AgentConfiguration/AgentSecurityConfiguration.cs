using Newtonsoft.Json;

namespace Photon.Agent.Internal.AgentConfiguration
{
    public class AgentSecurityConfiguration
    {
        [JsonProperty("enabled")]
        public bool Enabled {get; set;}

        [JsonProperty("domainEnabled")]
        public bool DomainEnabled {get; set;}
    }
}
