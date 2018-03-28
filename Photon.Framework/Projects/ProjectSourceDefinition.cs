using Newtonsoft.Json;
using System;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectSourceDefinition
    {
        [JsonProperty("type")]
        public string Type {get; set;}

        [JsonProperty("source")]
        public string Source {get; set;}
    }
}
