using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectBuildTask
    {
        [JsonProperty("name", Order = 1)]
        public string Name {get; set;}

        [JsonProperty("description", Order = 2)]
        public string Description {get; set;}

        [JsonProperty("refspec", Order = 3)]
        public string GitRefspec {get; set;}

        [JsonProperty("security", Order = 4)]
        public string Security {get; set;}

        [JsonProperty("agent", Order = 5)]
        public string AgentMode {get; set;}

        [JsonProperty("roles", Order = 6)]
        public List<string> Roles {get; set;}
    }
}
