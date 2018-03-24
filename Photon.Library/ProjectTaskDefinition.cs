using Newtonsoft.Json;
using System.Collections.Generic;

namespace Photon.Library
{
    public class ProjectTaskDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("type")]
        public string Type {get; set;}

        [JsonProperty("file")]
        public string File {get; set;}

        [JsonProperty("roles")]
        public List<string> Roles {get; set;}


        public ProjectTaskDefinition()
        {
            Roles = new List<string>();
        }
    }
}
