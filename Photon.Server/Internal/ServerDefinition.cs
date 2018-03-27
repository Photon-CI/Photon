using Newtonsoft.Json;
using System.Collections.Generic;

namespace Photon.Server.Internal
{
    public class ServerDefinition
    {
        [JsonProperty("http")]
        public ServerHttpDefinition Http {get; set;}

        [JsonProperty("agents")]
        public List<ServerAgentDefinition> Agents {get; set;}


        public ServerDefinition()
        {
            Http = new ServerHttpDefinition();
            Agents = new List<ServerAgentDefinition>();
        }
    }
}
