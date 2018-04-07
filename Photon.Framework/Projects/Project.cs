using Newtonsoft.Json;
using System;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class Project
    {
        [JsonProperty("id")]
        public string Id {get; set;}

        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("description")]
        public string Description {get; set;}

        [JsonProperty("sourceType")]
        public string SourceType {get; set;}

        [JsonProperty("sourcePath")]
        public string SourcePath {get; set;}

        [JsonProperty("preBuild")]
        public string PreBuild {get; set;}

        [JsonProperty("postBuild")]
        public string PostBuild {get; set;}
    }
}
