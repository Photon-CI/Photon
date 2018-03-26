using Newtonsoft.Json;

namespace Photon.Agent.Internal
{
    public class AgentDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("http")]
        public AgentHttpDefinition Http {get; set;}


        public AgentDefinition()
        {
            Http = new AgentHttpDefinition();
        }
    }
}
