using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectEnvironmentDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("agents")]
        public List<string> Agents {get; set;}


        public ProjectEnvironmentDefinition()
        {
            Agents = new List<string>();
        }
    }
}
