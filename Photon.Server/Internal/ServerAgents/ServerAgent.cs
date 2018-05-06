using System.Collections.Generic;

namespace Photon.Server.Internal.Agents
{
    internal class ServerAgent
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string TcpHost {get; set;}
        public int TcpPort {get; set;}
        public List<string> Roles {get; set;}


        public ServerAgent()
        {
            Roles = new List<string>();
        }
    }
}
