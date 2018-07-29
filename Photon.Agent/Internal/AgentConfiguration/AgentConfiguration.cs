namespace Photon.Agent.Internal.AgentConfiguration
{
    internal class AgentConfiguration
    {
        public string Name {get; set;}
        public AgentHttpConfiguration Http {get; set;}
        public AgentTcpConfiguration Tcp {get; set;}
        public AgentSecurityConfiguration Security {get; set;}


        public AgentConfiguration()
        {
            Http = new AgentHttpConfiguration {
                Host = "*",
                Port = 8080,
                Path = "/photon/agent",
            };

            Tcp = new AgentTcpConfiguration {
                Host = "0.0.0.0",
                Port = 10930,
            };
        }
    }
}
