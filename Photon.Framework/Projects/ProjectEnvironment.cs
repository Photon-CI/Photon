using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectEnvironment
    {
        [JsonProperty("name", Order = 1)]
        public string Name {get; set;}

        [JsonProperty("agents", Order = 2)]
        public List<string> AgentIdList {get; set;}


        public ProjectEnvironment()
        {
            AgentIdList = new List<string>();
        }
    }
}
