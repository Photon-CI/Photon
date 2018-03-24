using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Library
{
    public class ProjectDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("environments")]
        public List<ProjectEnvironmentDefinition> Environments {get; set;}

        [JsonProperty("tasks")]
        public List<ProjectTaskDefinition> Tasks {get; set;}


        public ProjectDefinition()
        {
            Environments = new List<ProjectEnvironmentDefinition>();
            Tasks = new List<ProjectTaskDefinition>();
        }

        public ProjectTaskDefinition FindTask(string taskName)
        {
            return Tasks?.FirstOrDefault(x => string.Equals(x.Name, taskName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
