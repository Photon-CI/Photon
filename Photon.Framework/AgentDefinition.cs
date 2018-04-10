using Newtonsoft.Json;

namespace Photon.Framework
{
    public class AgentDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("http")]
        public AgentHttpDefinition Http {get; set;}

        [JsonProperty("tcp")]
        public AgentTcpDefinition Tcp {get; set;}


        public AgentDefinition()
        {
            Http = new AgentHttpDefinition();
            Tcp = new AgentTcpDefinition();
        }
    }
}
