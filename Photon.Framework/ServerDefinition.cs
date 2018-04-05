using Newtonsoft.Json;
using System.Collections.Generic;

namespace Photon.Framework
{
    public class ServerDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

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
