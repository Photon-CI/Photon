using Newtonsoft.Json;
using System;

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

        [JsonIgnore]
        public IProjectSource Source {get; set;}

        [JsonProperty("assembly")]
        public string AssemblyFile {get; set;}
    }
}
