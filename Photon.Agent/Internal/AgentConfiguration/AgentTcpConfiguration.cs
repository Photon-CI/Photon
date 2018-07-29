using Newtonsoft.Json;

namespace Photon.Agent.Internal.AgentConfiguration
{
    internal class AgentTcpConfiguration
    {
        [JsonProperty("host")]
        public string Host {get; set;}

        [JsonProperty("port")]
        public int Port {get; set;}
    }
}
