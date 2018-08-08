using Newtonsoft.Json;

namespace Photon.Server.Internal.ServerConfiguration
{
    internal class ServerConfiguration
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("http")]
        public ServerHttpConfiguration Http {get; set;}

        [JsonProperty("security")]
        public ServerSecurityConfiguration Security {get; set;}


        public ServerConfiguration()
        {
            Http = new ServerHttpConfiguration {
                Host = "*",
                Port = 8080,
                Path = "/photon/server",
            };

            Security = new ServerSecurityConfiguration {
                Enabled = false,
            };
        }
    }
}
