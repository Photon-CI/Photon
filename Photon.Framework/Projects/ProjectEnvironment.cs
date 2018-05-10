using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectEnvironment
    {
        public string Name {get; set;}

        [JsonProperty("agents")]
        public List<string> AgentIdList {get; set;}


        public ProjectEnvironment()
        {
            AgentIdList = new List<string>();
        }
    }
}
