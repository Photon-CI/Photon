using Newtonsoft.Json;
using System.Collections.Generic;

namespace Photon.Library.Models
{
    public class AgentStartTaskRequest
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("type")]
        public string Type {get; set;}

        [JsonProperty("file")]
        public string File {get; set;}

        [JsonProperty("roles")]
        public List<string> Roles {get; set;}


        public AgentStartTaskRequest()
        {
            Roles = new List<string>();
        }
    }
}
