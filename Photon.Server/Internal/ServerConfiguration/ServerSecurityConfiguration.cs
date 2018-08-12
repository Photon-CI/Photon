using Newtonsoft.Json;

namespace Photon.Server.Internal.ServerConfiguration
{
    public class ServerSecurityConfiguration
    {
        [JsonProperty("enabled")]
        public bool Enabled {get; set;}

        [JsonProperty("domainEnabled")]
        public bool DomainEnabled {get; set;}
    }
}
