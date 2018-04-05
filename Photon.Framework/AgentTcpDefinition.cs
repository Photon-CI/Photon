using Newtonsoft.Json;

namespace Photon.Framework
{
    public class AgentTcpDefinition
    {
        [JsonProperty("host")]
        public string Host {get; set;}

        [JsonProperty("port")]
        public int Port {get; set;}
    }
}
