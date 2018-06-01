using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class Project
    {
        [JsonProperty("id", Order = 1)]
        public string Id {get; set;}

        [JsonProperty("name", Order = 2)]
        public string Name {get; set;}

        [JsonProperty("description", Order = 3)]
        public string Description {get; set;}

        [JsonProperty("assembly", Order = 4)]
        public string AssemblyFile {get; set;}

        [JsonProperty("preBuild", Order = 5)]
        public string PreBuild {get; set;}

        [JsonProperty("maxBuilds", Order = 6)]
        public uint? MaxBuilds {get; set;}

        [JsonProperty("source", Order = 7)]
        [JsonConverter(typeof(ProjectSourceSerializer))]
        public IProjectSource Source {get; set;}

        [JsonProperty("environments", Order = 8)]
        public List<ProjectEnvironment> Environments {get; set;}


        public Project()
        {
            Environments = new List<ProjectEnvironment>();
        }
    }
}
