using Newtonsoft.Json;

namespace Photon.Server.Internal.ServerConfiguration
{
    public class ServerHttpDefinition
    {
        [JsonProperty("host")]
        public string Host {get; set;}

        [JsonProperty("port")]
        public int Port {get; set;}

        [JsonProperty("path")]
        public string Path {get; set;}
    }
}
