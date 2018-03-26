using Newtonsoft.Json;
using System.Collections.Generic;

namespace Photon.Library.Models
{
    public class ServerStartResponse
    {
        [JsonProperty("taskId")]
        public string TaskId {get; set;}

        [JsonProperty("agents")]
        public List<ServerStartAgentResponse> Agents {get; set;}
    }
}
