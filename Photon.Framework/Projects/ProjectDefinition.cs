using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Projects
{
    public class ProjectDefinition
    {
        [JsonProperty("id")]
        public string Id {get; set;}

        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("source")]
        public ProjectSourceDefinition Source {get; set;}

        [JsonProperty("environments")]
        public List<ProjectEnvironmentDefinition> Environments {get; set;}

        [JsonProperty("jobs")]
        public List<ProjectJobDefinition> Jobs {get; set;}


        public ProjectDefinition()
        {
            Environments = new List<ProjectEnvironmentDefinition>();
            Jobs = new List<ProjectJobDefinition>();
        }

        public ProjectJobDefinition FindJob(string jobName)
        {
            return Jobs?.FirstOrDefault(x => string.Equals(x.Name, jobName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
