using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class Project
    {
        [JsonProperty(Order = 1)]
        public string Id {get; set;}

        [JsonProperty(Order = 2)]
        public string Name {get; set;}

        [JsonProperty(Order = 3)]
        public string Description {get; set;}

        [JsonProperty("assembly", Order = 4)]
        public string AssemblyFile {get; set;}

        [JsonProperty(Order = 5)]
        public string PreBuild {get; set;}

        [JsonProperty(Order = 6)]
        [JsonConverter(typeof(ProjectSourceSerializer))]
        public IProjectSource Source {get; set;}

        [JsonProperty(Order = 7)]
        public List<ProjectEnvironment> Environments {get; set;}


        public Project()
        {
            Environments = new List<ProjectEnvironment>();
        }
    }
}
