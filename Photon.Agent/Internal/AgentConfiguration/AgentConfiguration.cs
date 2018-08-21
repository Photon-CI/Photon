using Newtonsoft.Json;

namespace Photon.Agent.Internal.AgentConfiguration
{
    internal class AgentConfiguration
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("http")]
        public AgentHttpConfiguration Http {get; set;}

        [JsonProperty("tcp")]
        public AgentTcpConfiguration Tcp {get; set;}

        [JsonProperty("applications")]
        public AgentApplicationConfiguration Applications {get; set;}

        [JsonProperty("security")]
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

            Applications = new AgentApplicationConfiguration {
                MaxCount = 10,
            };

            Security = new AgentSecurityConfiguration {
                Enabled = false,
            };
        }
    }
}
