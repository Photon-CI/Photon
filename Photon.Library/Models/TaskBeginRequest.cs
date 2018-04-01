using Newtonsoft.Json;

namespace Photon.Library.Models
{
    public class TaskBeginRequest
    {
        [JsonProperty("sessionId")]
        public string SessionId {get; set;}

        [JsonProperty("taskName")]
        public string TaskName {get; set;}
    }
}
