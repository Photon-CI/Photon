using Newtonsoft.Json;

namespace Photon.Library.Models
{
    public class TaskBeginResponse
    {
        [JsonProperty("taskId")]
        public string TaskId {get; set;}
    }
}
