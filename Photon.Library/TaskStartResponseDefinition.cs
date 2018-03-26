using Newtonsoft.Json;

namespace Photon.Library
{
    public class TaskStartResponseDefinition
    {
        [JsonProperty("successful")]
        public bool Successful {get; set;}

        [JsonProperty("message")]
        public string Message {get; set;}

        [JsonProperty("agent")]
        public TaskStartResponseAgentDefinition Agent {get; set;}
    }
}
