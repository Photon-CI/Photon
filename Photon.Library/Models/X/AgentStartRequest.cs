using Newtonsoft.Json;

namespace Photon.Library.Models
{
    public class AgentStartRequest
    {
        [JsonProperty("project-name")]
        public string ProjectName {get; set;}

        [JsonProperty("task-name")]
        public string TaskName {get; set;}
    }
}
