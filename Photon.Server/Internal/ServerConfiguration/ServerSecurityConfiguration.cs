using Newtonsoft.Json;

namespace Photon.Server.Internal.ServerConfiguration
{
    public class ServerSecurityDefinition
    {
        [JsonProperty("enabled")]
        public bool Enabled {get; set;}

        [JsonProperty("type")]
        public string Type {get; set;}
    }
}
