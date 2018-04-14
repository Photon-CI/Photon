using System.Collections.Generic;

namespace Photon.Framework
{
    public class ServerDefinition
    {
        public string Name {get; set;}
        public ServerHttpDefinition Http {get; set;}
        public List<ServerAgentDefinition> Agents {get; set;}


        public ServerDefinition()
        {
            Http = new ServerHttpDefinition();
            Agents = new List<ServerAgentDefinition>();
        }
    }
}
