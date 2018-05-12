using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class Project
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
        public string SourceType {get; set;}
        public string PreBuild {get; set;}

        [JsonConverter(typeof(ProjectSourceSerializer))]
        public IProjectSource Source {get; set;}

        [JsonProperty("assembly")]
        public string AssemblyFile {get; set;}

        public List<ProjectEnvironment> Environments {get; set;}


        public Project()
        {
            Environments = new List<ProjectEnvironment>();
        }
    }
}
