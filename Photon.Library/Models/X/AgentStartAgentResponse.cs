using Newtonsoft.Json;

namespace Photon.Library.Models
{
    public class AgentStartAgentResponse
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("taskId")]
        public string TaskId {get; set;}
    }
}
