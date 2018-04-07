using Newtonsoft.Json;

namespace Photon.Server.Internal
{
    internal class BuildStartInfo
    {
        [JsonProperty("project")]
        public string ProjectId {get; set;}

        [JsonProperty("assembly")]
        public string AssemblyFile {get; set;}

        [JsonProperty("task")]
        public string TaskName {get; set;}

        [JsonProperty("refspec")]
        public string GitRefspec {get; set;}

        [JsonProperty("roles")]
        public string[] Roles {get; set;}
    }
}
