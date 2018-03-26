using Newtonsoft.Json;
using System.Collections.Generic;

namespace Photon.Library.Models
{
    public class AgentStartResponse
    {
        [JsonProperty("taskId")]
        public string TaskId {get; set;}

        [JsonProperty("agents")]
        public List<AgentStartAgentResponse> Agents {get; set;}
    }
}
