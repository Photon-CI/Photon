using Newtonsoft.Json;

namespace Photon.Agent.Internal
{
    internal class AgentTcpDefinition
    {
        [JsonProperty("host")]
        public string Host {get; set;}

        [JsonProperty("port")]
        public int Port {get; set;}
    }
}
