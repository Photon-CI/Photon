using Newtonsoft.Json;

namespace Photon.Server.Internal
{
    internal class BuildStartInfo
    {
        [JsonProperty("project")]
        public string ProjectId {get; set;}

        [JsonProperty("assembly")]
        public string AssemblyFile {get; set;}

        [JsonProperty("refspec")]
        public string GitRefspec {get; set;}

        [JsonProperty("script")]
        public string ScriptName {get; set;}
    }
}
