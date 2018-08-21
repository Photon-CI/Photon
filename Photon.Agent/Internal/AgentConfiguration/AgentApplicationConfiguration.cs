using Newtonsoft.Json;

namespace Photon.Agent.Internal.AgentConfiguration
{
    internal class AgentApplicationConfiguration
    {
        [JsonProperty("maxCount")]
        public int MaxCount {get; set;}
    }
}
