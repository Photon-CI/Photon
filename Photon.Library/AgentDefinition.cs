using Newtonsoft.Json;

namespace Photon.Library
{
    public class AgentDefinition
    {
        [JsonProperty("http")]
        public ServerHttpDefinition Http {get; set;}


        public AgentDefinition()
        {
            Http = new ServerHttpDefinition();
        }
    }
}
